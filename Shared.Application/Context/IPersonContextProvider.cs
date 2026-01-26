namespace Shared.Application.Context
{
    public interface IPersonContextProvider
    {
        PersonContext? Current { get; set; }
    }
}
