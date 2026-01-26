using CoreAPI.Middleware;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

namespace CoreAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CoreAPI",
                    Version = "v1"
                });

                // Add JWT Bearer token authentication
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
                });

                // Add personId header as a required parameter for all endpoints
                options.AddSecurityDefinition("personSyncId", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "personSyncId",
                    Description = "Person ID header (required for all endpoints except those marked with [SkipPersonIdCheck])"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "personSyncId"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddScoped<PersonContextResolver>();

            return services;
        }

        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment = null)
        {
            // CORS Configuration
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? new[] { "*" };

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("X-RateLimit-Remaining", "X-RateLimit-Reset");
                });

                // Strict policy for production
                options.AddPolicy("StrictPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                          .WithHeaders("Authorization", "Content-Type", "personSyncId")
                          .AllowCredentials()
                          .WithExposedHeaders("X-RateLimit-Remaining", "X-RateLimit-Reset");
                });
            });

            // Rate Limiting
            services.AddRateLimiter(options =>
            {
                // Global rate limit policy
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    // Get client identifier (IP address or user ID)
                    var clientId = context.Connection.RemoteIpAddress?.ToString() 
                        ?? context.User?.Identity?.Name 
                        ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientId,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100, // Requests per window
                            Window = TimeSpan.FromMinutes(1) // Time window
                        });
                });

                // Policy for authentication endpoints (stricter)
                options.AddPolicy("AuthPolicy", context =>
                {
                    var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientId,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5, // 5 login attempts per window
                            Window = TimeSpan.FromMinutes(15) // 15 minute window
                        });
                });

                // Policy for API endpoints
                options.AddPolicy("ApiPolicy", context =>
                {
                    var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientId,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 1000, // Higher limit for authenticated users
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // Rejection response
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync(
                        "Rate limit exceeded. Please try again later.", 
                        cancellationToken: token);
                };
            });

            // HSTS (HTTP Strict Transport Security) - Only in production
            if (environment == null || !environment.IsDevelopment())
            {
                services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });
            }

            // Antiforgery (CSRF Protection)
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "__Host-CSRF";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
            });

            return services;
        }
    }
}
