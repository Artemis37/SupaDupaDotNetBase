using Microsoft.Extensions.Logging;
using Shared.Application.Context;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Vehicle.Application.Decorators;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AuditAttribute : Attribute, IDecoratorAttribute
{
}

public sealed class AuditDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly PersonContext _personContext;
    private readonly ILogger<AuditDecorator<TCommand>> _logger;

    public AuditDecorator(ICommandHandler<TCommand> handler, PersonContext personContext, ILogger<AuditDecorator<TCommand>> logger)
    {
        _handler = handler;
        _personContext = personContext;
        _logger = logger;
    }

    public async Task<Result> Handle(TCommand command)
    {
        var commandName = typeof(TCommand).Name;
        var userId = _personContext.PersonId;
        
        _logger.LogInformation("Audit: User {UserId} executing {CommandName}", userId, commandName);
        
        var result = await _handler.Handle(command);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Audit: User {UserId} completed {CommandName} successfully", userId, commandName);
        }
        else
        {
            _logger.LogWarning("Audit: User {UserId} failed {CommandName}: {Error}", userId, commandName, result.Error);
        }
        
        return result;
    }
}
