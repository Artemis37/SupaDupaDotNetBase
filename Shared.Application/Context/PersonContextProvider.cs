namespace Shared.Application.Context
{
    public static class PersonContextProvider
    {
        private static readonly AsyncLocal<PersonContext?> _current = new();

        public static PersonContext? Current => _current.Value;

        public static void SetCurrent(PersonContext? context)
        {
            _current.Value = context;
        }
    }
}
