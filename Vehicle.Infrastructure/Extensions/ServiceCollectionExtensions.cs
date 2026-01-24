using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Infrastructure.Context;
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
            // Register MasterDbContext
            var masterConnectionString = configuration.GetConnectionString("Master");
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                throw new InvalidOperationException("Master connection string is not configured.");
            }

            services.AddDbContext<MasterDbContext>(options =>
                options.UseSqlServer(masterConnectionString));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();

            services.AddScoped<IDbContextFactory, DbContextFactory>();
            services.AddScoped<IShardingDbContextProvider, ShardingDbContextProvider>();

            services.AddScoped<ShardingDbContext>(sp =>
            {
                var provider = sp.GetRequiredService<IShardingDbContextProvider>();
                return provider.GetDbContext();
            });

            return services;
        }
    }
}
