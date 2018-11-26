using System;
using System.Threading.Tasks;
using ContosoUniversity.Core.Auth;
using ContosoUniversity.Services.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoUniversity.Web.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public Task<RegisterUser.Response> RegisterUser([FromBody]RegisterUser.Request request)
            => _mediator.Send(request ?? new RegisterUser.Request());

        [HttpGet]
        public Task<QueryUsers.Response> QueryUsers([FromQuery]QueryUsers.Request request)
            => _mediator.Send(request ?? new QueryUsers.Request());

        [HttpGet("{id}")]
        public Task<QueryUser.Response> QueryUser([FromRoute]Guid id)
            => _mediator.Send(new QueryUser.Request { Id = id });

        [HttpPut("{id}/activate")]
        public Task<ActivateUser.Response> ActivateUser([FromRoute]Guid id)
            => _mediator.Send(new ActivateUser.Request { Id = id });

        [HttpPut("{id}/deactivate")]
        public Task<DeactivateUser.Response> DeactivateUser([FromRoute]Guid id)
            => _mediator.Send(new DeactivateUser.Request { Id = id });

        [HttpPut("{id}")]
        public Task<UpdateUser.Response> UpdateUser([FromRoute]Guid id, [FromBody]UpdateUser.Request request)
        {
            request.Id = id;
            return _mediator.Send(request ?? new UpdateUser.Request());
        }

        [HttpPut("{id}/password")]
        public Task<ChangePassword.Response> ChangePassword([FromRoute]Guid id, [FromBody]ChangePassword.Request request)
        {
            request = request ?? new ChangePassword.Request();
            request.Id = id;
            return _mediator.Send(request);
        }
    }
}
