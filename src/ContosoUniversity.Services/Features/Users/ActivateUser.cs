using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core.Users;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Users
{
    public class ActivateUser
    {
        public class Request : IRequest<Response>
        {
            public Guid Id { get; set; }
        }

        public class Response
        {
        }

        public class Handler : MediatR.IRequestHandler<Request, Response>
        {
            private readonly IDocumentSession _session;
            public Handler(IDocumentSession session)
            {
                _session = session;
            }
            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                var user = _session.Load<User>(request.Id);
                if (user == null)
                    throw new UserNotExistException();

                user.Activate();

                _session.Store(user);
                return Task.FromResult(new Response());
            }
        }
    }
}
