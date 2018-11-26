using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoUniversity.Core.Enrollments
{
    public class Enrollment : RootEntity
    {
        public Enrollment(Guid studentId, Guid courseId)
        {
            CourseId = courseId;
            StudentId = studentId;
            EnrollmentDateUtc = DateTime.UtcNow;
        }

        public Guid StudentId { get; private set; }
        public Guid CourseId { get; private set; }
        public DateTime EnrollmentDateUtc { get; private set; }
        public int? Grade { get; private set; }

        public void AssignGrade(int grade)
        {
            if (!IsValidGrade(grade))
                throw new CoreException($"{grade} is an invalid grade. Supported are from 5 to 10.");

            Grade = grade;
        }

        private static bool IsValidGrade(int grade)
        {
            return grade >= 5 && grade <= 10;
        }
    }
}
