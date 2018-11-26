using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoUniversity.Core.Users
{
    public enum UserStatus
    {
        /// <summary>
        /// Registered, but their account is not activated. Waiting to verify their account thru the email 
        /// verification link.
        /// </summary>
        RegisteredNotActivated = 0,
        Activated = 1,
        Unregistered = 2,
        Deactivated = 10,
    }
}
