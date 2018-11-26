using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Auth;
using ContosoUniversity.Core.Users;
using FluentValidation;
using Marten;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ContosoUniversity.Services.Features.Auth
{
    public class CreateToken
    {
        public class Request : IRequest<Response>
        {
            public string Grant_type { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Refresh_token { get; set; }
        }

        public class Response
        {
            public string Access_token { get; set; }
            public string Refresh_token { get; set; }
            public string Expires_in { get; set; }
        }

        public class CreateTokenValidator : AbstractValidator<Request>
        {
            public CreateTokenValidator()
            {
                RuleFor(x => x.Grant_type).Must(x => x == "password" || x == "refresh_token")
                    .WithMessage("Not supported grant type");

                RuleFor(x => x.Username)
                    .Must((request, value) => request.Grant_type != "password" || string.IsNullOrEmpty(value) == false)
                    .WithMessage("username error");

                RuleFor(x => x.Password)
                    .Must((request, value) => request.Grant_type != "password" || string.IsNullOrEmpty(value) == false)
                    .WithMessage("password error");

                RuleFor(x => x.Refresh_token)
                    .Must((request, value) => request.Grant_type != "refresh_token" || string.IsNullOrEmpty(value) == false)
                    .WithMessage("refresh_token error");
            }
        }

        public class Handler : IRequestHandler<Request, Response>
        {
            private readonly IDocumentSession _session;
            private readonly IConfiguration _config;

            public Handler(IDocumentSession session, IConfiguration config)
            {
                _session = session;
                _config = config;
            }

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                Response response;
                if (request.Grant_type == "password")
                {
                    response = await TryCreateAccessToken(request.Username, request.Password);
                    if (response == null)
                        throw new CoreException("Invalid credentials");
                }
                else if (request.Grant_type == "refresh_token")
                {
                    response = await TryRefreshAccessToken(request.Refresh_token);
                    if (response == null)
                        throw new CoreException("Invalid refresh token");
                }
                else
                    throw new CoreException("Unsupported grant_type. Please use 'password' or 'refresh_token' as grant_type.");

                return response;
            }

            private Response BuildAccessToken(User user)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("username", user.Email),
                    new Claim("email", user.Email),
                    new Claim("fullname", user.Fullname),
                    new Claim("avatarUri", user.AvatarUri),
                    new Claim("isAdmin", user.IsAdmin.ToString())
                };

                // create access token
                var expiresInSeconds = (int)TimeSpan.FromMinutes(30).TotalSeconds;
                var token = new JwtSecurityToken(
                    _config["Jwt:Issuer"],
                    _config["Jwt:Issuer"],
                    expires: DateTime.UtcNow.AddSeconds(expiresInSeconds),
                    signingCredentials: creds,
                    claims: claims);

                // create refresh token
                var refreshExpiresInSeconds = (int)TimeSpan.FromDays(15).TotalSeconds;
                var jwtRefreshToken = new JwtSecurityToken(
                    _config["Jwt:Issuer"],
                    _config["Jwt:Issuer"],
                    expires: DateTime.UtcNow.AddSeconds(refreshExpiresInSeconds),
                    signingCredentials: creds,
                    claims: claims);

                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                var refreshToken = new JwtSecurityTokenHandler().WriteToken(jwtRefreshToken);

                var refreshTokenTicket = new RefreshTokenTicket(refreshToken, user.Id, refreshExpiresInSeconds);
                _session.Store(refreshTokenTicket);

                return new Response
                {
                    Access_token = accessToken,
                    Refresh_token = refreshToken,
                    Expires_in = expiresInSeconds.ToString()
                };
            }

            public async Task<Response> TryCreateAccessToken(string username, string password)
            {
                var user = await _session.Query<User>().FirstOrDefaultAsync(x => x.Email == username.ToLowerInvariant());
                if (user == null || !user.CanLogin(password))
                    return null;

                return BuildAccessToken(user);
            }

            public async Task<Response> TryRefreshAccessToken(string refreshToken)
            {
                var refreshTokenTicket = await _session.Query<RefreshTokenTicket>()
                    .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

                if (refreshTokenTicket == null || refreshTokenTicket.ExpirationDate < DateTime.UtcNow)
                    return null;

                var user = await _session.Query<User>().FirstOrDefaultAsync(x => x.Id == refreshTokenTicket.UserId);
                if (user == null)
                    return null;

                _session.Eject(refreshTokenTicket);
                return BuildAccessToken(user);
            }
        }
    }
}
