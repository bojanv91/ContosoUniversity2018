using System;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Students;
using FluentValidation;
using Marten;
using MediatR;
using Newtonsoft.Json;

namespace ContosoUniversity.Services.Features.Students
{
    public class UpdateStudent
    {
        public class Request : IRequest<Response>
        {
            [JsonIgnore]
            public Guid Id { get; set; }
            public string Fullname { get; set; }
        }

        public class Response
        {
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Fullname).NotEmpty();
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

                student.Fullname = request.Fullname;

                _session.Store(student);
                return new Response();
            }
        }
    }
}
