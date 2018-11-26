using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Students
{
    public class QueryStudents
    {
        public class Request : IRequest<Response>
        {
            public string SearchTerm { get; set; } = "";
        }

        public class Response
        {
            public IEnumerable<Item> Items { get; set; } = Enumerable.Empty<Item>();

            public class Item
            {
                public Guid Id { get; set; }
                public string Fullname { get; set; }
                public DateTime EnrollmentDate { get; set; }
                public int TotalCoursesEnrolled { get; set; }
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
                var sql = @"
SELECT
    student.id                                      AS Id,
    student.data->>'Fullname'                       AS Fullname,
    (student.data->>'EnrollmentDateUtc')::TIMESTAMP AS EnrollmentDate,
    COUNT(enrollment.id)                            AS TotalCoursesEnrolled
FROM mt_doc_student student
LEFT JOIN mt_doc_enrollment enrollment ON enrollment.student_id = student.id
WHERE student.data->>'Fullname' ~* @SearchTerm
GROUP BY student.id
";
                var items = await _session.Connection.QueryAsync<Response.Item>(sql, new
                {
                    SearchTerm = request.SearchTerm
                });

                return new Response { Items = items };
            }
        }
    }
}
