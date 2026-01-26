using Shared.Application.Context;

namespace Shared.Infrastructure.Context
{
    public class PersonContextProvider : IPersonContextProvider
    {
        private static AsyncLocal<PersonContext?> _current = new();

        public PersonContext? GetPersonContext()
        {
            return _current.Value;
        }

        public void SetPersonContext(PersonContext? context)
        {
            _current.Value = context;
        }
    }
}
