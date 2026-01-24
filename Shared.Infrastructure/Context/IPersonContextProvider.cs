using Shared.Infrastructure.Context;

namespace Shared.Infrastructure.Context
{
    public interface IPersonContextProvider
    {
        PersonContext GetContext();
    }
}
