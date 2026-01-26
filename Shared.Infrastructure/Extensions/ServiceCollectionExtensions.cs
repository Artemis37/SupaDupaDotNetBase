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
            services.AddScoped<IPersonContextProvider, PersonContextProvider>();
            services.AddScoped<PersonContext>(sp =>
            {
                var accessor = sp.GetRequiredService<IPersonContextProvider>();
                var context = accessor.GetPersonContext();
                if (context == null)
                {
                    context = new PersonContext();
                    accessor.SetPersonContext(context);
                }
                return context;
            });
            services.AddScoped<IDbContextFactory, DbContextFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
