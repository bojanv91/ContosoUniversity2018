using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoUniversity.Core.Users
{
    public class UserNotExistException : CoreException
    {
        public UserNotExistException() : base("User not exist!")
        {
        }
    }

    public class HaventInsertedFullnameOrEmailYetException : CoreException
    {
        public HaventInsertedFullnameOrEmailYetException() : base("You haven't inserted fullname and email yet. Please insert it in order to be changed!")
        {
        }
    }

    public class ThereAreNoUsersException : CoreException
    {
        public ThereAreNoUsersException() : base("There are no users yet!")
        {
        }
    }
}
