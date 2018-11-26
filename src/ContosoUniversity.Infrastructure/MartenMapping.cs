using ContosoUniversity.Core.Auth;
using ContosoUniversity.Core.Courses;
using ContosoUniversity.Core.Enrollments;
using ContosoUniversity.Core.Students;
using ContosoUniversity.Core.Users;
using Marten;

namespace ContosoUniversity.Infrastructure
{
    public class MartenMapping : MartenRegistry
    {
        public MartenMapping()
        {
            For<User>()
                .Duplicate(x => x.Email);

            For<RefreshTokenTicket>()
                .ForeignKey<User>(x => x.UserId);

            For<Student>();
            For<Course>();

            For<Enrollment>()
                .ForeignKey<Student>(x => x.StudentId)
                .ForeignKey<Course>(x => x.CourseId);
        }
    }
}
