using Microsoft.EntityFrameworkCore;
using Shared.Application.Context;
using Vehicle.Infrastructure.Data;

namespace CoreAPI.Middleware
{
    public class PersonContextResolver
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly IPersonContextProvider _personContextProvider;

        public PersonContextResolver(
            MasterDbContext masterDbContext,
            IPersonContextProvider personContextProvider)
        {
            _masterDbContext = masterDbContext;
            _personContextProvider = personContextProvider;
        }

        public async Task<PersonContextResolutionResult> ResolvePersonContextAsync(int personId)
        {
            // TODO: Add caching to resolve person context by personId to avoid querying the master database multiple times

            var personMaster = await _masterDbContext.PersonMasters
                .FirstOrDefaultAsync(pm => pm.Id == personId);

            if (personMaster == null)
            {
                return PersonContextResolutionResult.NotFound("Person not found");
            }

            var personContext = _personContextProvider.GetContext();
            personContext.PersonId = personId;
            personContext.ShardId = personMaster.ShardId;

            return PersonContextResolutionResult.Success();
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
