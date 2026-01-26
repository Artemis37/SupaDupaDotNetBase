using Shared.Application.Context;

namespace Shared.Infrastructure.Context
{
    public class PersonContextProvider : IPersonContextProvider
    {
        // TODO: convert to AsyncLocal later
        private static PersonContext? _current = new();

        public PersonContext? Current
        {
            get => _current;
            set => _current = value;
        }
    }
}
