using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoUniversity.Core.Auth
{
    public class RefreshTokenTicket : RootEntity
    {
        public string RefreshToken { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime ExpirationDate { get; private set; }

        public RefreshTokenTicket(string refreshToken, Guid userId, long expiresInSeconds)
        {
            RefreshToken = refreshToken;
            UserId = userId;
            ExpirationDate = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        }
    }
}
