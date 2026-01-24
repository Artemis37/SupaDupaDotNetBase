using System.Text;
using CoreAPI.Attributes;
using Microsoft.AspNetCore.Http;

namespace CoreAPI.Middleware
{
    public class PersonContextMiddleware
    {
        private readonly RequestDelegate _next;
        private const string PersonIdHeaderName = "personId";

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

            if (!context.Request.Headers.TryGetValue(PersonIdHeaderName, out var personIdHeaderValue))
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

            var resolutionResult = await personContextResolver.ResolvePersonContextAsync(personId);

            if (!resolutionResult.IsSuccess)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\": \"{resolutionResult.ErrorMessage}\"}}", Encoding.UTF8);
                return;
            }

            await _next(context);
        }
    }
}
