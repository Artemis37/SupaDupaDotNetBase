using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Extensions;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Infrastructure.Data;
using Vehicle.Infrastructure.Repositories;

namespace Vehicle.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSharedInfrastructure();

            services.AddScoped<IShardingDbContextProvider<ShardingDbContext>, Shared.Infrastructure.Data.ShardingDbContextProvider<ShardingDbContext>>();
            services.AddScoped<ShardingDbContext>(sp =>
            {
                var provider = sp.GetRequiredService<IShardingDbContextProvider<ShardingDbContext>>();
                try
                {
                    return provider.GetDbContext();
                }
                catch (InvalidOperationException)
                {
                    // Return null when PersonContext is not set (e.g., during person creation)
                    return null!;
                }
            });
            services.AddScoped<IUnitOfWork>(sp =>
            {
                var provider = sp.GetRequiredService<IShardingDbContextProvider<ShardingDbContext>>();
                return provider.GetDbContext();
            });

            var masterConnectionString = configuration.GetConnectionString("Master");
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                throw new InvalidOperationException("Master connection string is not configured.");
            }

            services.AddDbContext<MasterDbContext>(options => 
                options.UseSqlServer(masterConnectionString, sqlServerOptions => 
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null)));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IPersonMasterRepository, PersonMasterRepository>();

            return services;
        }
    }
}
