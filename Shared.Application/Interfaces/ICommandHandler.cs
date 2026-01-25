using Shared.Application.Models;

namespace Shared.Application.Interfaces;

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<Result> Handle(TCommand command);
}
