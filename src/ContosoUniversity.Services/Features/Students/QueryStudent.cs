using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Enrollments;
using ContosoUniversity.Core.Students;
using ContosoUniversity.Core.Users;
using Dapper;
using FluentValidation;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Students
{
    public class QueryStudent
    {
        public class Request : IRequest<Response>
        {
            public Guid Id { get; set; }
        }

        public class Response
        {
            public string Fullname { get; set; }
            public DateTime EnrollmentDate { get; set; }
            public int TotalDaysEnrolled => (int)(DateTime.UtcNow - EnrollmentDate).TotalDays;
            public int TotalCoursesEnrolled => Enrollments.Count();
            public IEnumerable<EnrollmentItem> Enrollments { get; set; } = Enumerable.Empty<EnrollmentItem>();

            public class EnrollmentItem
            {
                public Guid EnrollmentId { get; set; }
                public int? Grade { get; set; }

                public Guid CourseId { get; set; }
                public string CourseTitle { get; set; }
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
                var student = await _session.LoadStrictAsync<Student>(request.Id);

                var enrollmentsSql = @"
SELECT
    enrollment.id               AS EnrollmentId,
    enrollment.data->>'Grade'   AS Grade,
    course.id                   AS CourseId,
    course.data->>'Title'       AS CourseTitle
FROM mt_doc_enrollment enrollment
INNER JOIN mt_doc_course course ON course.id = enrollment.course_id
INNER JOIN mt_doc_student student ON student.id = enrollment.student_id
WHERE enrollment.student_id = @StudentId
";
                var enrollments = await _session.Connection.QueryAsync<Response.EnrollmentItem>(enrollmentsSql, new
                {
                    StudentId = student.Id
                });

                return new Response
                {
                    Fullname = student.Fullname,
                    EnrollmentDate = student.EnrollmentDateUtc,
                    Enrollments = enrollments
                };
            }
        }
    }
}
