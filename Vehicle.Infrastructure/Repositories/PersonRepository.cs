using Shared.Application.Interfaces;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Models;

namespace Vehicle.Infrastructure.Repositories
{
    public class PersonRepository : BaseRepository<Person>, IPersonRepository
    {
        public PersonRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
