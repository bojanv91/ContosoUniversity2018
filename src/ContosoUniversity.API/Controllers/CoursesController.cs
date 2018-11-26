using System;
using System.Threading.Tasks;
using ContosoUniversity.Services.Features.Courses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContosoUniversity.Web.Controllers
{
    [Route("api/[controller]")]
    public class CoursesController : Controller
    {
        private readonly IMediator _mediator;

        public CoursesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public Task<CreateOrUpdateCourse.Response> CreateCourse([FromBody]CreateOrUpdateCourse.Request request) =>
            _mediator.Send(request ?? new CreateOrUpdateCourse.Request());

        [HttpGet]
        public Task<QueryCourses.Response> QueryCourses([FromQuery]QueryCourses.Request request) =>
            _mediator.Send(request ?? new QueryCourses.Request());

        [HttpGet("{id}")]
        public Task<QueryCourse.Response> QueryCourse([FromRoute]Guid id) =>
            _mediator.Send(new QueryCourse.Request { Id = id });

        [HttpPut("{id}")]
        public Task<CreateOrUpdateCourse.Response> UpdateCourse([FromRoute]Guid id, [FromBody]CreateOrUpdateCourse.Request request)
        {
            request = request ?? new CreateOrUpdateCourse.Request();
            request.Id = id;
            return _mediator.Send(request);
        }
    }
}
