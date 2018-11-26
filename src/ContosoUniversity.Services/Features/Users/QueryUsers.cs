using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Users
{
    public class QueryUsers
    {
        public class Request : IRequest<Response>
        {
        }

        public class Response
        {
            public IEnumerable<Item> Items { get; set; } = Enumerable.Empty<Item>();

            public class Item
            {
                public Guid Id { get; set; }
                public string Fullname { get; set; }
                public string Email { get; set; }
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
    id                  AS Id,
    data->>'Fullname'   AS Fullname,
    data->>'Email'      AS Email
FROM mt_doc_user
";
                return new Response
                {
                    Items = await _session.Connection.QueryAsync<Response.Item>(sql)
                };
            }
        }
    }
}
