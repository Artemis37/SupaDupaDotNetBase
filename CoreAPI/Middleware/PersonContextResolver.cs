using Microsoft.EntityFrameworkCore;
using Shared.Application.Context;
using Vehicle.Infrastructure.Data;

namespace CoreAPI.Middleware
{
    public class PersonContextResolver
    {
        private readonly MasterDbContext _masterDbContext;

        public PersonContextResolver(MasterDbContext masterDbContext)
        {
            _masterDbContext = masterDbContext;
        }

        public async Task<PersonContext?> ResolvePersonContextAsync(Guid personSyncId)
        {
            // TODO: Add caching to resolve person context by personSyncId to avoid querying the master database multiple times

            var personMaster = await _masterDbContext.PersonMasters
                .FirstOrDefaultAsync(pm => pm.PersonSyncId == personSyncId);

            if (personMaster == null)
            {
                return null;
            }

            var personContext = new PersonContext
            {
                PersonId = null, // Will be set later from shard DB
                ShardId = personMaster.ShardId
            };

            return personContext;
        }
    }

    public class PersonContextResolutionResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }

        private PersonContextResolutionResult(bool isSuccess, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static PersonContextResolutionResult Success() => new PersonContextResolutionResult(true);
        public static PersonContextResolutionResult NotFound(string message) => new PersonContextResolutionResult(false, message);
    }
}
