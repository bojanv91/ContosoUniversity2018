using FluentValidation;
using ContosoUniversity.Core.Users;
using Marten;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ContosoUniversity.Services.Features.Users
{
    public class ChangePassword
    {
        public class Request : IRequest<Response>
        {
            [JsonIgnore]
            public Guid Id { get; set; }
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }

        public class Response 
        {
        }

        public class ChangePasswordValidator : AbstractValidator<Request>
        {
            public ChangePasswordValidator()
            {
                RuleFor(x => x.NewPassword).MinimumLength(6);
                RuleFor(x => x.CurrentPassword).NotEmpty();
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
                var user = await _session.LoadAsync<User>(request.Id);
                user.ChangePassword(request.NewPassword, request.CurrentPassword);

                _session.Store(user);
                return new Response();
            }
        }
    }
}
