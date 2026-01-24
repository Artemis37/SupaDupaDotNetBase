using System.Text;
using CoreAPI.Attributes;
using Vehicle.Domain.Interfaces.Services;

namespace CoreAPI.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string AuthorizationHeaderName = "Authorization";
        private const string BearerPrefix = "Bearer ";

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IAuthService authService)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var skipAttribute = endpoint.Metadata.GetMetadata<SkipAuthenticationAttribute>();
                if (skipAttribute != null)
                {
                    await _next(context);
                    return;
                }
            }

            if (!context.Request.Headers.TryGetValue(AuthorizationHeaderName, out var authHeaderValue))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Authorization header is required\"}", Encoding.UTF8);
                return;
            }

            var authHeader = authHeaderValue.ToString();
            
            if (!authHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Authorization header must be Bearer token\"}", Encoding.UTF8);
                return;
            }

            var token = authHeader.Substring(BearerPrefix.Length).Trim();
            
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Bearer token is empty\"}", Encoding.UTF8);
                return;
            }

            if (!authService.ValidateToken(token, out int userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Invalid or expired token\"}", Encoding.UTF8);
                return;
            }

            context.Items["UserId"] = userId;

            await _next(context);
        }
    }
}
