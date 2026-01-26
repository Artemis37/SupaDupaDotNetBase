using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Infrastructure;
using Vehicle.Application.Decorators;

namespace Vehicle.Application.Infrastructure;

public class HandlersRegistration : HandlersRegistrationBase
{
    public override string AssemblyName => "Vehicle.Application";

    public HandlersRegistration(IServiceCollection services) : base(services)
    {
    }

    protected override Type ToDecorator(object attribute)
    {
        Type type = attribute.GetType();
        
        if (type == typeof(LoggingCommandAttribute))
            return typeof(LoggingCommandDecorator<>);
        
        if (type == typeof(LoggingQueryAttribute))
            return typeof(LoggingQueryDecorator<,>);
        
        if (type == typeof(ValidationAttribute))
            return typeof(ValidationDecorator<>);
        
        if (type == typeof(AuditAttribute))
            return typeof(AuditDecorator<>);

        throw new ArgumentException($"Unknown decorator attribute: {attribute}");
    }
}
