using CoreAPI.Middleware;
using Microsoft.OpenApi.Models;

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

                // Add personId header as a required parameter for all endpoints
                options.AddSecurityDefinition("personId", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "personId",
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
                                Id = "personId"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // PersonContext and IPersonContextProvider are now registered in Shared.Infrastructure
            services.AddScoped<PersonContextResolver>();

            return services;
        }
    }
}
