using System;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core.Users;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Users
{
    public class DeactivateUser
    {
        public class Request : IRequest<Response>
        {
            public Guid Id { get; set; }
        }

        public class Response
        {
        }

        public class Handler : IRequestHandler<Request, Response>
        {
            private IDocumentSession _session { get; set; }

            public Handler(IDocumentSession session)
            {
                _session = session;
            }

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                var user = await _session.LoadAsync<User>(request.Id);
                if (user == null)
                    throw new UserNotExistException();

                user.Deactivate();

                _session.Store(user);
                return new Response();
            }
        }
    }
}
