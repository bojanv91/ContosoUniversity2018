using System;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Courses;
using FluentValidation;
using Marten;
using MediatR;
using Newtonsoft.Json;

namespace ContosoUniversity.Services.Features.Courses
{
    public class CreateOrUpdateCourse
    {
        public class Request : IRequest<Response>
        {
            [JsonIgnore]
            public Guid? Id { get; set; }
            public string Title { get; set; }
            public int Credits { get; set; }
        }

        public class Response
        {
            public Guid Id { get; set; }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.Credits).GreaterThan(0);
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
                var course = request.Id.HasValue    // is update
                    ? await _session.LoadAsync<Course>(request.Id.Value)
                    : new Course();

                course.Title = request.Title;
                course.Credits = request.Credits;

                _session.Store(course);
                return new Response { Id = course.Id };
            }
        }
    }
}

