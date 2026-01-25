using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Shared.Application.Infrastructure;

public sealed class Messages
{
    private readonly IServiceProvider _provider;

    public Messages(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<Result> Dispatch(ICommand command)
    {
        Type type = typeof(ICommandHandler<>);
        Type[] typeArgs = { command.GetType() };
        Type handlerType = type.MakeGenericType(typeArgs);

        using (var serviceScope = _provider.CreateScope())
        {
            dynamic handler = serviceScope.ServiceProvider.GetService(handlerType);
            
            if (handler == null)
                throw new InvalidOperationException($"No handler registered for {command.GetType().Name}");
            
            var result = await handler.Handle((dynamic)command);
            return result;
        }
    }

    public async Task<T> Dispatch<T>(IQuery<T> query)
    {
        Type type = typeof(IQueryHandler<,>);
        Type[] typeArgs = { query.GetType(), typeof(T) };
        Type handlerType = type.MakeGenericType(typeArgs);

        using (var serviceScope = _provider.CreateScope())
        {
            dynamic handler = serviceScope.ServiceProvider.GetService(handlerType);
            
            if (handler == null)
                throw new InvalidOperationException($"No handler registered for {query.GetType().Name}");
            
            T result = await handler.Handle((dynamic)query);
            return result;
        }
    }
}
