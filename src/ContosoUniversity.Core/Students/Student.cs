using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoUniversity.Core.Students
{
    public class Student : RootEntity
    {
        private Student()
        {
        }

        public static Student Enroll(string fullname, DateTime enrollmentDate)
        {
            if (enrollmentDate.ToUniversalTime() > DateTime.UtcNow)
                throw new CoreException("Cannot enroll student in future date.");

            return new Student
            {
                Fullname = fullname,
                EnrollmentDateUtc = enrollmentDate.ToUniversalTime()
            };
        }

        public string Fullname { get; set; }
        public DateTime EnrollmentDateUtc { get; private set; }
    }
}
