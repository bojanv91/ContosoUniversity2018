using System;
using System.Threading.Tasks;
using ContosoUniversity.Services.Features.Enrollments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContosoUniversity.Web.Controllers
{
    [Route("api/[controller]")]
    public class EnrollmentsController : Controller
    {
        private readonly IMediator _mediator;

        public EnrollmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public Task<EnrollStudentInCourse.Response> EnrollStudentInCourse([FromBody]EnrollStudentInCourse.Request request) =>
            _mediator.Send(request ?? new EnrollStudentInCourse.Request());

        [HttpPut("{id}/grade")]
        public Task<AssignGrade.Response> UpdateStudent([FromRoute]Guid id, [FromBody]AssignGrade.Request request)
        {
            request = request ?? new AssignGrade.Request();
            request.EnrollmentId = id;
            return _mediator.Send(request);
        }
    }
}
