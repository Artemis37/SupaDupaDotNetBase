using Microsoft.Extensions.Configuration;
using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Data
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly IConfiguration _configuration;

        public DbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateConnectionString(int shardId)
        {
            var baseConnectionString = _configuration.GetConnectionString("ShardingBase");
            if (string.IsNullOrEmpty(baseConnectionString))
            {
                throw new InvalidOperationException("ShardingBase connection string is not configured.");
            }

            // Append the database name based on shardId
            var databaseName = $"SupaDupaSharding{shardId}";
            
            // Check if connection string already has Database parameter
            if (baseConnectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase))
            {
                // Replace existing Database parameter
                var parts = baseConnectionString.Split(';');
                var newParts = parts.Where(p => !p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                newParts.Add($"Database={databaseName}");
                return string.Join(";", newParts);
            }
            else
            {
                // Append Database parameter
                return $"{baseConnectionString};Database={databaseName}";
            }
        }
    }
}
