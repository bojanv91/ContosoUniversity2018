using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Core;
using FluentValidation;
using Marten;
using MediatR;

namespace ContosoUniversity.Infrastructure.Behaviors
{
    public class DocumentSessionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IDocumentSession _session;

        public DocumentSessionBehavior(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var response = await next();

            await _session.SaveChangesAsync();

            return response;
        }
    }
}
