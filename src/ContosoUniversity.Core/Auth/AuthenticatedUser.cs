using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using ContosoUniversity.Core.Users;
using Microsoft.AspNetCore.Http;

namespace ContosoUniversity.Core.Auth
{
    public class AuthenticatedUser
    {
        public AuthenticatedUser()
        {
        }

        public AuthenticatedUser(User user)
        {
            Id = user.Id;
            Username = user.Email;
            Email = user.Email;
            Fullname = user.Fullname;
            AvatarUri = user.AvatarUri;
        }

        public static AuthenticatedUser GetCurrentUser(ClaimsPrincipal user)
        {
            return new AuthenticatedUser
            {
                Id = new Guid(user.FindFirst("id").Value),
                Username = user.FindFirst("username").Value,
                Email = user.FindFirst(ClaimTypes.Email).Value,
                Fullname = user.FindFirst("fullname").Value,
                AvatarUri = user.FindFirst("avatarUri").Value,
                IsAdmin = bool.Parse(user.FindFirst("isAdmin").Value)
            };
        }

        /// <summary>
        /// User's Id
        /// </summary>
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string AvatarUri { get; set; }
        public bool IsAdmin { get; set; }
    }
}
