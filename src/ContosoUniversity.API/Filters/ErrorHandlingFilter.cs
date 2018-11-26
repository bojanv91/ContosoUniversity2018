using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContosoUniversity.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Serilog;

namespace ContosoUniversity.Web.Filters
{
    /// <summary>
    /// Handles errors, validation and others.
    /// Read more about <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1#exception-filters">exception filters</see> in the official ASP.NET Core docs.
    /// </summary>
    public class ErrorHandlingFilter : ExceptionFilterAttribute
    {
        private static readonly ILogger _logger = Log.ForContext<ErrorHandlingFilter>();
        private readonly IHostingEnvironment _hostingEnvironment;

        public ErrorHandlingFilter(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public override void OnException(ExceptionContext context)
        {
            context.ExceptionHandled = true;

            var requestLog = CreateLogObject(context.HttpContext);
            var knownResponse = HandleExceptionIfKnown(context);
            if (knownResponse != null)
            {
                context.Result = knownResponse.Result;
                _logger.Information("Validation: {Type}. HttpStatusCode: {StatusCode}. Request: {@Request}. Errors: {@Errors}.",
                    context.Exception.GetType().Name,
                    knownResponse.StatusCode,
                    requestLog,
                    knownResponse.Errors);
            }
            else
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                _logger.Error(context.Exception, "Unhandled exception. Request: {Request}", requestLog);
            }
        }

        private static object CreateLogObject(HttpContext httpContext)
        {
            var httpRequest = httpContext.Request;
            return new
            {
                Call = httpRequest.Method + " " + httpRequest.Host.Value + httpRequest.Path.ToUriComponent() + httpRequest.QueryString.ToUriComponent(),
                Body = DeserializeFromStream(httpRequest.Body),
                ContentType = httpRequest.ContentType,
                UserIdentityName = httpContext.User?.Identity?.Name,
                RemoteIpAddress = httpRequest.HttpContext.Connection?.RemoteIpAddress?.ToString()
            };
        }

        private static object DeserializeFromStream(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(jsonTextReader);
            }
        }

        private static KnownResponseWrapper HandleExceptionIfKnown(ExceptionContext context)
        {
            if (context.Exception is RequestValidationException requestValidation)
                return new KnownResponseWrapper(new BadRequestObjectResult(requestValidation.Errors), requestValidation.Errors);

            if (context.Exception is CoreException core)
                return new KnownResponseWrapper(new BadRequestObjectResult(core.Errors), core.Errors);

            if (context.Exception is UnauthorizedAccessException)
                return new KnownResponseWrapper(new UnauthorizedResult(), "Unauthorized.");

            return null;
        }

        class KnownResponseWrapper
        {
            private KnownResponseWrapper(IActionResult result, int? statusCode, IEnumerable<ValidationError> errors)
            {
                Result = result;
                StatusCode = statusCode;
                Errors = errors;
            }

            public KnownResponseWrapper(StatusCodeResult result, string message)
                : this(result, result.StatusCode, new List<ValidationError>() { new ValidationError { ErrorMessage = message } })
            {
            }

            public KnownResponseWrapper(ObjectResult result, IEnumerable<ValidationError> errors)
                : this(result, result.StatusCode, errors)
            {
            }

            public int? StatusCode { get; set; }
            public IActionResult Result { get; set; }
            public IEnumerable<ValidationError> Errors { get; set; } = Enumerable.Empty<ValidationError>();
        }
    }
}
