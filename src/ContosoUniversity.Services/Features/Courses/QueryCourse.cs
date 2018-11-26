using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Courses;
using Marten;
using MediatR;
using Dapper;

namespace ContosoUniversity.Services.Features.Courses
{
    public class QueryCourse
    {
        public class Request : IRequest<Response>
        {
            public Guid Id { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int Credits { get; set; }
            public int TotalStudentsEnrolled => Enrollments.Count();
            public IEnumerable<EnrollmentItem> Enrollments { get; set; } = Enumerable.Empty<EnrollmentItem>();

            public class EnrollmentItem
            {
                public Guid EnrollmentId { get; set; }
                public int? Grade { get; set; }
                public Guid StudentId { get; set; }
                public string StudentFullname { get; set; }
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
                var course = await _session.LoadStrictAsync<Course>(request.Id);

                var studentEnrollmentsSql = @"
SELECT
    enrollment.id               AS EnrollmentId,
    enrollment.data->>'Grade'   AS Grade,
    student.id                  AS StudentId,
    student.data->>'Fullname'   AS StudentFullname
FROM mt_doc_enrollment enrollment
INNER JOIN mt_doc_course course ON course.id = enrollment.course_id
INNER JOIN mt_doc_student student ON student.id = enrollment.student_id
WHERE enrollment.course_id = @CourseId
";
                var enrollments = await _session.Connection.QueryAsync<Response.EnrollmentItem>(studentEnrollmentsSql, new
                {
                    CourseId = course.Id
                });

                return new Response
                {
                    Id = course.Id,
                    Title = course.Title,
                    Credits = course.Credits,
                    Enrollments = enrollments
                };
            }
        }
    }
}
