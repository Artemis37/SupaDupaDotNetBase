using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Context;
using Shared.Infrastructure.Data;

namespace Shared.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<PersonContext>();
            services.AddScoped<IPersonContextProvider, PersonContextProvider>();
            services.AddScoped<IDbContextFactory, DbContextFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
