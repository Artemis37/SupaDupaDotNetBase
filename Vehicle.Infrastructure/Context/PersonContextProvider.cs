namespace Vehicle.Infrastructure.Context
{
    public class PersonContextProvider : IPersonContextProvider
    {
        private readonly PersonContext _context;

        public PersonContextProvider()
        {
            _context = new PersonContext();
        }

        public PersonContext GetContext()
        {
            return _context;
        }
    }
}
