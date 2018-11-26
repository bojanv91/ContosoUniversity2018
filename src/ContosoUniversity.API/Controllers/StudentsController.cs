using System;
using System.Threading.Tasks;
using ContosoUniversity.Services.Features.Students;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContosoUniversity.Web.Controllers
{
    [Route("api/[controller]")]
    public class StudentsController : Controller
    {
        private readonly IMediator _mediator;

        public StudentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public Task<EnrollStudent.Response> EnrollStudent([FromBody]EnrollStudent.Request request) =>
            _mediator.Send(request ?? new EnrollStudent.Request());

        [HttpGet]
        public Task<QueryStudents.Response> QueryStudents([FromQuery]QueryStudents.Request request) =>
            _mediator.Send(request ?? new QueryStudents.Request());

        [HttpGet("{id}")]
        public Task<QueryStudent.Response> QueryStudent([FromRoute]Guid id) =>
            _mediator.Send(new QueryStudent.Request { Id = id });

        [HttpPut("{id}")]
        public Task<UpdateStudent.Response> UpdateStudent([FromRoute]Guid id, [FromBody]UpdateStudent.Request request)
        {
            request = request ?? new UpdateStudent.Request();
            request.Id = id;
            return _mediator.Send(request);
        }
    }
}
