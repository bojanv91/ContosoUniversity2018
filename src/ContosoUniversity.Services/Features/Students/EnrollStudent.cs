using System;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Students;
using ContosoUniversity.Core.Users;
using FluentValidation;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Students
{
    public class EnrollStudent
    {
        public class Request : IRequest<Response>
        {
            public string Fullname { get; set; }
            public DateTime EnrollmentDate { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
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

            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                var student = Student.Enroll(request.Fullname, request.EnrollmentDate);

                _session.Store(student);
                return Task.FromResult(new Response { Id = student.Id });
            }
        }
    }
}

