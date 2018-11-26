using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Courses
{
    public class QueryCourses
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
                public string Title { get; set; }
                public int Credits { get; set; }
                public int TotalStudentsEnrolled { get; set; }
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
    course.id                                      AS Id,
    course.data->>'Title'                          AS Title,
    course.data->>'Credits'                        AS Credits,
    COUNT(enrollment.id)                           AS TotalStudentsEnrolled
FROM mt_doc_course course
LEFT JOIN mt_doc_enrollment enrollment ON enrollment.course_id = course.id
WHERE course.data->>'Title' ~* @SearchTerm
GROUP BY course.id
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
