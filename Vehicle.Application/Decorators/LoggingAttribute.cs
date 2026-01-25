using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Vehicle.Application.Decorators;

// For Commands
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class LoggingCommandAttribute : Attribute, IDecoratorAttribute
{
}

public sealed class LoggingCommandDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly ILogger<LoggingCommandDecorator<TCommand>> _logger;

    public LoggingCommandDecorator(ICommandHandler<TCommand> handler, ILogger<LoggingCommandDecorator<TCommand>> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task<Result> Handle(TCommand command)
    {
        var commandName = typeof(TCommand).Name;
        _logger.LogInformation("Executing command: {CommandName}", commandName);
        
        var result = await _handler.Handle(command);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("Command {CommandName} failed: {Error}", commandName, result.Error);
        }
        else
        {
            _logger.LogInformation("Command {CommandName} completed successfully", commandName);
        }
        
        return result;
    }
}

// For Queries
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class LoggingQueryAttribute : Attribute, IDecoratorAttribute
{
}

public sealed class LoggingQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _handler;
    private readonly ILogger<LoggingQueryDecorator<TQuery, TResult>> _logger;

    public LoggingQueryDecorator(IQueryHandler<TQuery, TResult> handler, ILogger<LoggingQueryDecorator<TQuery, TResult>> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task<TResult> Handle(TQuery query)
    {
        var queryName = typeof(TQuery).Name;
        _logger.LogInformation("Executing query: {QueryName}", queryName);
        
        var result = await _handler.Handle(query);
        
        _logger.LogInformation("Query {QueryName} completed successfully", queryName);
        
        return result;
    }
}
