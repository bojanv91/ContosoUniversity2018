using ContosoUniversity.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoUniversity.Core.Users
{
    public class User : RootEntity, IUser
    {
        protected const int PASSWORD_RECOVERY_CODE_SIZE = 15;
        protected const int VERIFICATION_CODE_SIZE = 20;
        protected HashSet<string> _clientAccessList = new HashSet<string>();    // list of client_id where user is allowed

        protected User()
        {
        }

        protected User(string fullname, string email, string passwordPlain)
        {
            Fullname = fullname;
            Email = email.ToLowerInvariant();
            PasswordSalt = EncriptionUtil.GenerateSaltKey();
            PasswordHash = EncriptionUtil.GeneratePasswordHash(passwordPlain, this.PasswordSalt);
            PasswordLastChangedUtc = DateTime.UtcNow;
            PasswordRecoveryCode = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RegisteredOnUtc = DateTime.UtcNow;
            VerificationCode = Guid.NewGuid().ToString("n");
            AvatarUri = "DefaultAvatarURI";
        }

        /// <summary>
        /// Registers "normal" user.
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="email"></param>
        /// <param name="passwordPlain"></param>
        /// <returns></returns>
        public static User Register(string fullname, string email, string passwordPlain)
        {
            var user = new User(fullname, email, passwordPlain);
            return user;
        }

        /// <summary>
        /// Create SystemUser. A SystemUser is a user who can manage tenants in multi-tenant environment.
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="email"></param>
        /// <param name="passwordPlain"></param>
        /// <returns></returns>
        public static User CreateSystemUser(string fullname, string email, string passwordPlain)
        {
            var user = new User(fullname, email, passwordPlain);
            user.IsSystemUser = true;
            user.Status = UserStatus.Activated;
            return user;
        }

        private string _email;
        public string Email { get { return _email; } protected set { _email = value.ToLowerInvariant(); } }
        public string Fullname { get; set; }
        public string PasswordHash { get; private set; }
        public string PasswordSalt { get; private set; }
        public DateTime PasswordLastChangedUtc { get; private set; }
        public DateTime RegisteredOnUtc { get; private set; }
        public DateTime VerifiedOnUtc { get; private set; }
        public string PasswordRecoveryCode { get; private set; }
        public string VerificationCode { get; private set; }
        public string AvatarUri { get; set; } 
        public bool IsAdmin { get; set; } 

        /// <summary>
        /// SystemUser is super type of user who can manage tenants.
        /// </summary>
        public bool IsSystemUser { get; private set; }

        public UserStatus Status { get; protected set; }
        public UserStatus PreviousStatus { get; private set; }

        /// <summary>
        /// List of client_id string values to where user can login.
        /// </summary>
        public IEnumerable<string> ClientAccessList => _clientAccessList;

        public virtual bool CanLogin(string passwordPlain)
        {
            if (Status == UserStatus.Deactivated)
                return false;

            var loginPasswordHash = EncriptionUtil.GeneratePasswordHash(passwordPlain, PasswordSalt);
            return loginPasswordHash == PasswordHash;
        }

        /// <summary>
        /// Request password reset.
        /// </summary>
        public void ResetPassword()
        {
            PasswordRecoveryCode = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public void ChangePasswordByRecoveryCode(string newPasswordPlain, string passwordRecoveryCode)
        {
            if (PasswordRecoveryCode != passwordRecoveryCode)
                throw new CoreException("Invalid credentials.");

            PasswordSalt = EncriptionUtil.GenerateSaltKey();
            PasswordHash = EncriptionUtil.GeneratePasswordHash(newPasswordPlain, PasswordSalt);
            PasswordLastChangedUtc = DateTime.UtcNow;
            PasswordRecoveryCode = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public void ChangePassword(string newPassword, string oldPassword)
        {
            if (!CanLogin(oldPassword))
                throw new CoreException("_InvalidCredentialsException");

            PasswordSalt = EncriptionUtil.GenerateSaltKey();
            PasswordHash = EncriptionUtil.GeneratePasswordHash(newPassword, PasswordSalt);
            PasswordLastChangedUtc = DateTime.UtcNow;
            PasswordRecoveryCode = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public void ChangeEmail(string newEmail, string password)
        {
            if (!CanLogin(password))
                throw new CoreException("_InvalidCredentialsException");

            Email = newEmail;
        }

        public void Activate()
        {
            PreviousStatus = Status;
            Status = UserStatus.Activated;
            //DomainEvents.Raise(new EmployeeActivateEvent(Id));
        }

        public void Deactivate()
        {
            PreviousStatus = Status;
            Status = UserStatus.Deactivated;
            //DomainEvents.Raise(new EmployeeDeactivateEvent(Id));
        }

        public virtual bool TryActivateWithVerificationCode(string verificationCode)
        {
            if (VerificationCode != verificationCode)
                return false;

            Activate();
            VerifiedOnUtc = DateTime.UtcNow;
            return true;
        }

        public void GrantAccessToClient(string client_id)
            => _clientAccessList.Add(client_id);

        public void GrantAccessToClients(params string[] client_ids)
        {
            foreach (var id in client_ids)
                _clientAccessList.Add(id);
        }

        public bool HasAccessToClient(string client_id)
            => _clientAccessList.Contains(client_id);
    }
}
