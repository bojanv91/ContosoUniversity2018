using System;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Users;
using FluentValidation;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Users
{
    public class RegisterUser
    {
        public class Request : IRequest<Response>
        {
            public string Fullname { get; set; }

            string _email;
            public string Email { get => _email; set => _email = value.ToLowerInvariant(); }

            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Fullname).NotEmpty();
                RuleFor(x => x.Email).EmailAddress();
                RuleFor(x => x.Password).MinimumLength(6);
                RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
            }
        }

        public class Handler : IRequestHandler<Request, Response>
        {
            private readonly IDocumentSession _session;
            public Handler(IDocumentSession session)
            {
                _session = session;
            }

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                bool isTaken = await _session.Query<User>().AnyAsync(x => x.Email == request.Email);
                if (isTaken)
                    throw new CoreException("Email already taken. Please sign in, or register with new email.");

                var user = User.Register(request.Fullname, request.Email, request.Password);
                user.GrantAccessToClient("webapp");

                _session.Store(user);
                return new Response { Id = user.Id };
            }
        }
    }
}

