using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Infrastructure;
using Vehicle.Application.Infrastructure;
using Vehicle.Application.Models;
using Vehicle.Application.Services;
using Vehicle.Domain.Interfaces.Services;

namespace Vehicle.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<ShardingSettings>(configuration.GetSection("Sharding"));
            services.AddScoped<IAuthService, AuthService>();

            // Register handlers and decorators
            new HandlersRegistration(services);
            
            // Register dispatcher
            services.AddSingleton<Messages>();

            return services;
        }
    }
}
