using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Vehicle.Application.Decorators;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class TransactionAttribute : Attribute, IDecoratorAttribute
{
}

public sealed class TransactionDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionDecorator(ICommandHandler<TCommand> handler, IUnitOfWork unitOfWork)
    {
        _handler = handler;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(TCommand command)
    {
        var result = await _handler.Handle(command);
        
        if (result.IsFailure)
            return result;
        
        var saved = await _unitOfWork.SaveChangesAsync();
        if (saved == 0)
            return Result.Fail("Failed to save changes to database");
        
        return result;
    }
}
