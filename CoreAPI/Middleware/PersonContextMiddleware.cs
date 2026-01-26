using CoreAPI.Attributes;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Context;

namespace CoreAPI.Middleware
{
    public class PersonContextMiddleware
    {
        private readonly RequestDelegate _next;
        private const string PERSON_ID_HEADER_NAME = "personId";

        public PersonContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            PersonContextResolver personContextResolver)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var skipAttribute = endpoint.Metadata.GetMetadata<SkipPersonIdCheckAttribute>();
                if (skipAttribute != null)
                {
                    await _next(context);
                    return;
                }
            }

            if (!context.Request.Headers.TryGetValue(PERSON_ID_HEADER_NAME, out var personIdHeaderValue))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"personId header is required\"}", Encoding.UTF8);
                return;
            }

            if (!int.TryParse(personIdHeaderValue.ToString(), out var personId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"personId must be a valid integer\"}", Encoding.UTF8);
                return;
            }

            var personContext = await personContextResolver.ResolvePersonContextAsync(personId);

            if (personContext == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Person not found\"}", Encoding.UTF8);
                return;
            }

            var personContextProvider = context.RequestServices.GetRequiredService<IPersonContextProvider>();
            personContextProvider.SetPersonContext(personContext);

            await _next(context);
        }
    }
}
