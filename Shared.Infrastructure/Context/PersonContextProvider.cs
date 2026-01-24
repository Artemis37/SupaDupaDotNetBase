using Shared.Application.Context;

namespace Shared.Infrastructure.Context
{
    public class PersonContextProvider : IPersonContextProvider
    {
        private readonly PersonContext _personContext;

        public PersonContextProvider(PersonContext personContext)
        {
            _personContext = personContext;
        }

        public PersonContext GetContext()
        {
            return _personContext;
        }
    }
}
