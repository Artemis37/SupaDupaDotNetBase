using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
