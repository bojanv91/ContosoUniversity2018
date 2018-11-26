using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using ContosoUniversity.Core.Users;
using Marten;
using MediatR;

namespace ContosoUniversity.Services.Features.Users
{
    public class QueryUser
    {
        public class Request : IRequest<Response>
        {
            public Guid Id { get; set; }
        }

        public class Response
        {
            public string Fullname { get; set; }
            public string Email { get; set; }
            public DateTime RegisteredOnUtc { get; set; }
            public bool IsActive { get; set; }
            public string PasswordRecoveryCode { get; set; }
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

                var response = new Response
                {
                    Fullname = user.Fullname,
                    Email = user.Email,
                    RegisteredOnUtc = user.RegisteredOnUtc,
                    IsActive = user.Status == UserStatus.Activated,
                    PasswordRecoveryCode = user.PasswordRecoveryCode
                };

                return Task.FromResult(response);
            }
        }
    }
}
