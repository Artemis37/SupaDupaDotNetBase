using CoreAPI.Attributes;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Context;
using Vehicle.Domain.Interfaces.Repositories;

namespace CoreAPI.Middleware
{
    public class PersonContextMiddleware
    {
        private readonly RequestDelegate _next;
        private const string PERSON_SYNC_ID_HEADER_NAME = "personSyncId";

        public PersonContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            PersonContextResolver personContextResolver,
            IServiceProvider serviceProvider)
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

            if (!context.Request.Headers.TryGetValue(PERSON_SYNC_ID_HEADER_NAME, out var personSyncIdHeaderValue))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"personSyncId header is required\"}", Encoding.UTF8);
                return;
            }

            if (!Guid.TryParse(personSyncIdHeaderValue.ToString(), out var personSyncId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"personSyncId must be a valid GUID\"}", Encoding.UTF8);
                return;
            }

            var personContext = await personContextResolver.ResolvePersonContextAsync(personSyncId);

            if (personContext == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Person not found\"}", Encoding.UTF8);
                return;
            }

            PersonContextProvider.SetCurrent(personContext);

            var personRepository = serviceProvider.GetRequiredService<IPersonRepository>();
            var person = await personRepository.GetByPersonSyncIdAsync(personSyncId);

            if (person == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Person not found in shard database\"}", Encoding.UTF8);
                return;
            }

            personContext.PersonId = person.Id;
            PersonContextProvider.SetCurrent(personContext);

            await _next(context);
        }
    }
}
