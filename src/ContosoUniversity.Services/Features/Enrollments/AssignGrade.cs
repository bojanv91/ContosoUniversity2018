using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Enrollments;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Enrollments
{
    public class AssignGrade
    {
        public class Request : IRequest<Response>
        {
            public Guid EnrollmentId { get; set; }
            public int Grade { get; set; }
        }

        public class Response
        {
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
                var enrollment = await _session.LoadStrictAsync<Enrollment>(request.EnrollmentId);

                enrollment.AssignGrade(request.Grade);

                _session.Store(enrollment);
                return new Response();
            }
        }
    }
}
