using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using ContosoUniversity.Core.Users;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Users
{
    public class UpdateUser
    {
        public class Request : IRequest<Response>
        {
            public Guid Id { get; set; }
            public string Fullname { get; set; }
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

            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                var user = _session.Load<User>(request.Id);
                if (user == null)
                    throw new UserNotExistException();

                user.Fullname = request.Fullname;

                _session.Store(user);
                return Task.FromResult(new Response());
            }
        }
    }
}
