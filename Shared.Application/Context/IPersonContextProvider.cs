namespace Shared.Application.Context
{
    public interface IPersonContextProvider
    {
        PersonContext? GetPersonContext();
        void SetPersonContext(PersonContext? context);
    }
}
