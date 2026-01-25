using Shared.Application.Interfaces;
using Shared.Application.Models;
using System.ComponentModel.DataAnnotations;

namespace Vehicle.Application.Decorators;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ValidationAttribute : Attribute, IDecoratorAttribute
{
}

public sealed class ValidationDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _handler;

    public ValidationDecorator(ICommandHandler<TCommand> handler)
    {
        _handler = handler;
    }

    public async Task<Result> Handle(TCommand command)
    {
        // Use DataAnnotations for validation
        var validationContext = new ValidationContext(command);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(command, validationContext, validationResults, true);
        
        if (!isValid)
        {
            var errors = string.Join(", ", validationResults.Select(e => e.ErrorMessage));
            return Result.Fail($"Validation failed: {errors}");
        }
        
        return await _handler.Handle(command);
    }
}
