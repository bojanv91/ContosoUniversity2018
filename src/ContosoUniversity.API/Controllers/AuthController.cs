using System.Threading.Tasks;
using ContosoUniversity.Services.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoUniversity.Web.Controllers
{
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public Task<CreateToken.Response> CreateToken([FromBody]CreateToken.Request request) =>
            _mediator.Send(request ?? new CreateToken.Request());
    }
}
