using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Application.Context;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Context;

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
