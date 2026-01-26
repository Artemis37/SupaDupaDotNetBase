using Microsoft.EntityFrameworkCore;
using Shared.Application.Context;
using Vehicle.Infrastructure.Data;

namespace CoreAPI.Extensions
{
    public static class DatabaseInitializationExtensions
    {
        public static async Task InitializeDatabasesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                // Migrate Master Database
                logger.LogInformation("Starting Master database migration...");
                var masterContext = services.GetRequiredService<MasterDbContext>();
                await masterContext.Database.MigrateAsync();
                logger.LogInformation("Master database migration completed successfully.");

                // Migrate all Sharding Databases
                var totalShards = configuration.GetValue<int>("Sharding:TotalShards");
                var baseConnectionString = configuration.GetConnectionString("ShardingBase");

                if (string.IsNullOrEmpty(baseConnectionString))
                {
                    throw new InvalidOperationException("ShardingBase connection string is not configured.");
                }

                logger.LogInformation("Starting migration for {TotalShards} shard databases...", totalShards);

                for (int shardId = 1; shardId <= totalShards; shardId++)
                {
                    try
                    {
                        logger.LogInformation("Migrating shard database {ShardId}...", shardId);

                        // Build connection string for this shard
                        var shardConnectionString = $"{baseConnectionString}Database=SupaDupaSharding{shardId};";

                        // Create DbContext for this shard
                        var optionsBuilder = new DbContextOptionsBuilder<ShardingDbContext>();
                        optionsBuilder.UseSqlServer(shardConnectionString, sqlServerOptions => 
                            sqlServerOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null));

                        // Create a dummy PersonContext for migration
                        var personContext = new PersonContext();

                        using var shardContext = new ShardingDbContext(optionsBuilder.Options, personContext);
                        await shardContext.Database.MigrateAsync();

                        logger.LogInformation("Shard database {ShardId} migration completed successfully.", shardId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error migrating shard database {ShardId}", shardId);
                        continue;
                    }
                }

                logger.LogInformation("All database migrations completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating databases.");
                throw;
            }
        }
    }
}
