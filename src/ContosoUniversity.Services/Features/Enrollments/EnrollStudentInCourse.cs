using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Courses;
using ContosoUniversity.Core.Enrollments;
using ContosoUniversity.Core.Students;
using FluentValidation;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Enrollments
{
    public class EnrollStudentInCourse
    {
        public class Request : IRequest<Response>
        {
            public Guid StudentId { get; set; }
            public Guid CourseId { get; set; }
        }

        public class Response
        {
            public Guid EnrollmentId { get; set; }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.StudentId).NotEmpty();
                RuleFor(x => x.CourseId).NotEmpty();
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
                var student = await _session.LoadStrictAsync<Student>(request.StudentId);
                var course = await _session.LoadStrictAsync<Course>(request.CourseId);

                if (await IsAlreadyEnrolledAsync(student, course))
                    throw new CoreException($"{student.Fullname} is already enrolled in {course.Title}.");

                var enrollment = new Enrollment(student.Id, course.Id);

                _session.Store(enrollment);
                return new Response { EnrollmentId = enrollment.Id };
            }

            private async Task<bool> IsAlreadyEnrolledAsync(Student student, Course course)
            {
                return await _session.Query<Enrollment>()
                    .AnyAsync(x => x.StudentId == student.Id && x.CourseId == course.Id);
            }
        }
    }
}
