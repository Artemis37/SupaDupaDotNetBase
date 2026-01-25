# Custom Decorator Pipeline - SupaDupaBase

## Overview

This project implements a custom command/query handler system with **attribute-based decorator pipeline** support. The pattern allows you to apply cross-cutting concerns (logging, validation, database transactions, etc.) to handlers using simple attributes.

## Architecture

```
Controller → Messages Dispatcher → Decorator Pipeline → Handler → Repository → UnitOfWork → DbContext
```

### Key Components

1. **Commands & Queries**: Represent operations (ICommand for writes, IQuery<T> for reads)
2. **Handlers**: Execute the business logic (ICommandHandler<T>, IQueryHandler<T, TResult>)
3. **Decorators**: Apply cross-cutting concerns (Logging, Validation, Transaction, Audit)
4. **Messages Dispatcher**: Routes commands/queries to their handlers
5. **Result Pattern**: Explicit success/failure tracking with error messages

## Benefits

- **Separation of Concerns**: Business logic separated from cross-cutting concerns
- **Reusability**: Decorators can be applied to any command/query
- **Testability**: Each handler and decorator can be tested in isolation
- **Maintainability**: Adding new decorators doesn't affect existing code
- **Type Safety**: Compile-time checking of command/query types
- **Performance**: Decorator resolution happens once at startup via reflection

## Available Decorators

### 1. LoggingAttribute

Logs command/query execution start, completion, and errors.

```csharp
[Logging]
public class MyCommandHandler : ICommandHandler<MyCommand>
{
    // Automatically logs: "Executing command: MyCommand"
    // and "MyCommand completed successfully" or failure
}
```

### 2. ValidationAttribute

Validates commands using DataAnnotations before execution.

```csharp
public class CreateVehicleCommand : ICommand
{
    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string LicensePlate { get; set; }
}

[Validation] // Automatically validates the command
public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand>
{
    // Only executes if validation passes
}
```

### 3. TransactionAttribute

Wraps handler execution in database transaction and auto-saves changes.

```csharp
[Transaction] // Automatically calls UnitOfWork.SaveChangesAsync()
public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand>
{
    // Changes are automatically persisted if Result.Ok() is returned
}
```

### 4. AuditAttribute

Logs who executed what command and when using PersonContext.

```csharp
[Audit] // Logs: "User {userId} executing {commandName}"
public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand>
{
    // Automatically tracks user actions
}
```

## How to Create a New Command

### Step 1: Define the Command

```csharp
using Shared.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Vehicle.Application.Commands;

public class CreateVehicleCommand : ICommand
{
    public VehicleType Type { get; set; }
    
    [Required(ErrorMessage = "License plate is required")]
    [StringLength(20, MinimumLength = 1)]
    public string LicensePlate { get; set; } = string.Empty;
}
```

### Step 2: Create the Handler

```csharp
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Vehicle.Application.Decorators;

namespace Vehicle.Application.Commands;

[Logging]
[Validation]
[Transaction]
public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly PersonContext _personContext;

    public CreateVehicleCommandHandler(IVehicleRepository vehicleRepository, PersonContext personContext)
    {
        _vehicleRepository = vehicleRepository;
        _personContext = personContext;
    }

    public async Task<Result> Handle(CreateVehicleCommand command)
    {
        var vehicle = new Domain.Models.Vehicle
        {
            PersonId = _personContext.PersonId ?? 0,
            Type = command.Type,
            LicensePlate = command.LicensePlate,
            CreatedBy = _personContext.PersonId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _vehicleRepository.AddAsync(vehicle);
        
        return Result.Ok();
    }
}
```

### Step 3: Use in Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Infrastructure;

[ApiController]
[Route("api/[controller]")]
public class VehicleController : ControllerBase
{
    private readonly Messages _messages;

    public VehicleController(Messages messages)
    {
        _messages = messages;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
    {
        var command = new CreateVehicleCommand
        {
            Type = request.Type,
            LicensePlate = request.LicensePlate
        };

        var result = await _messages.Dispatch(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Vehicle created successfully" });
    }
}
```

## How to Create a New Query

### Step 1: Define the Query

```csharp
using Shared.Application.Interfaces;

namespace Vehicle.Application.Queries;

public class GetAllVehiclesQuery : IQuery<List<VehicleDto>>
{
    // No properties needed for "get all" queries
}
```

### Step 2: Create the Handler

```csharp
using Shared.Application.Interfaces;
using Vehicle.Application.Decorators;

namespace Vehicle.Application.Queries;

[Logging]
public class GetAllVehiclesQueryHandler : IQueryHandler<GetAllVehiclesQuery, List<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly PersonContext _personContext;

    public GetAllVehiclesQueryHandler(IVehicleRepository vehicleRepository, PersonContext personContext)
    {
        _vehicleRepository = vehicleRepository;
        _personContext = personContext;
    }

    public async Task<List<VehicleDto>> Handle(GetAllVehiclesQuery query)
    {
        var vehicles = await _vehicleRepository.GetAllAsync();
        
        return vehicles
            .Where(v => v.PersonId == _personContext.PersonId && !v.IsDeleted)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                PersonId = v.PersonId,
                Type = v.Type,
                LicensePlate = v.LicensePlate,
                CreatedAt = v.CreatedAt
            })
            .ToList();
    }
}
```

### Step 3: Use in Controller

```csharp
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var query = new GetAllVehiclesQuery();
    var result = await _messages.Dispatch(query);

    return Ok(result);
}
```

## Decorator Execution Order

Decorators are applied in **reverse order** of how they appear on the handler class:

```csharp
[Logging]       // Executes 1st (outermost)
[Validation]    // Executes 2nd
[Transaction]   // Executes 3rd
public class MyCommandHandler : ICommandHandler<MyCommand>
{
    // Executes 4th (innermost)
}
```

**Flow**:
1. LoggingDecorator → Logs "Executing command"
2. ValidationDecorator → Validates command properties
3. TransactionDecorator → Begins transaction context
4. Handler → Executes business logic
5. TransactionDecorator → Saves changes to database
6. LoggingDecorator → Logs "Command completed successfully"

## How to Create a Custom Decorator

### Step 1: Create Attribute

```csharp
using Shared.Application.Interfaces;

namespace Vehicle.Application.Decorators;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class MyCustomAttribute : Attribute, IDecoratorAttribute
{
}
```

### Step 2: Create Decorator Class

The decorator name MUST follow the convention: `{AttributeName}Decorator<TCommand>`

```csharp
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Vehicle.Application.Decorators;

public sealed class MyCustomDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IDependency _dependency;

    public MyCustomDecorator(ICommandHandler<TCommand> handler, IDependency dependency)
    {
        _handler = handler;
        _dependency = dependency;
    }

    public async Task<Result> Handle(TCommand command)
    {
        // Before handler execution
        await DoSomethingBefore();
        
        // Execute handler
        var result = await _handler.Handle(command);
        
        // After handler execution
        if (result.IsSuccess)
        {
            await DoSomethingAfter();
        }
        
        return result;
    }

    private async Task DoSomethingBefore() { /* ... */ }
    private async Task DoSomethingAfter() { /* ... */ }
}
```

### Step 3: Apply to Handler

```csharp
[Logging]
[MyCustom]  // Your new decorator
[Transaction]
public class MyCommandHandler : ICommandHandler<MyCommand>
{
    // ...
}
```

## Integration with Existing Infrastructure

### UnitOfWork

The TransactionDecorator automatically uses the existing `IUnitOfWork` interface:
- Calls `SaveChangesAsync()` after successful handler execution
- Skips save if handler returns `Result.Fail()`

### PersonContext

Decorators and handlers have access to the current user's context:
- `_personContext.PersonId` - Current user's ID
- `_personContext.ShardId` - Current shard ID for database routing

### Sharding Support

The decorator pipeline works seamlessly with the existing database sharding:
- Commands/queries receive `PersonContext` via dependency injection
- Handlers use repositories that auto-route to the correct shard
- TransactionDecorator ensures transaction scope includes the correct DbContext

## Project Structure

```
Shared.Application/
├── Interfaces/
│   ├── ICommand.cs
│   ├── ICommandHandler.cs
│   ├── IQuery.cs
│   ├── IQueryHandler.cs
│   └── IDecoratorAttribute.cs
├── Models/
│   └── Result.cs
└── Infrastructure/
    ├── Messages.cs
    └── HandlersRegistrationBase.cs

Vehicle.Application/
├── Commands/
│   └── CreateVehicleCommand.cs
├── Queries/
│   └── GetAllVehiclesQuery.cs
├── Decorators/
│   ├── LoggingAttribute.cs
│   ├── ValidationAttribute.cs
│   ├── TransactionAttribute.cs
│   └── AuditAttribute.cs
└── Infrastructure/
    └── HandlersRegistration.cs

CoreAPI/
└── Controllers/
    └── VehicleController.cs
```

## Result Pattern

Instead of throwing exceptions or returning nulls, handlers return a `Result` object:

### For Commands (void-like operations):

```csharp
// Success
return Result.Ok();

// Failure
return Result.Fail("Vehicle with this license plate already exists");
```

### For Queries (with return value):

```csharp
// Success with value
return Result<VehicleDto>.Ok(vehicleDto);

// Failure
return Result<VehicleDto>.Fail("Vehicle not found");
```

### Checking Results:

```csharp
var result = await _messages.Dispatch(command);

if (result.IsFailure)
    return BadRequest(new { error = result.Error });

return Ok(new { message = "Success" });
```

## Performance Considerations

- **Startup**: Reflection overhead occurs only during service registration
- **Runtime**: Minimal overhead - just method calls through decorator chain
- **Memory**: One decorator wrapper instance created per request scope
- **Optimization**: Decorator resolution is cached by DI container

## Troubleshooting

### Handler not found

**Error**: `No handler registered for {CommandName}`

**Solutions**:
- Ensure handler class name ends with "Handler"
- Verify `HandlersRegistration.AssemblyName` matches "Vehicle.Application"
- Check that handler implements `ICommandHandler<>` or `IQueryHandler<,>`

### Decorator not executing

**Error**: Decorator doesn't run or throws exception

**Solutions**:
- Check attribute implements `IDecoratorAttribute`
- Verify naming convention: `MyAttribute` → `MyDecorator`1`
- Ensure decorator constructor parameters are registered in DI

### DI resolution fails

**Error**: `Type {TypeName} not found in DI container`

**Solutions**:
- Register all decorator dependencies in DI (e.g., ILogger, IUnitOfWork)
- Check that dependencies are scoped correctly (Scoped for per-request, Singleton for global)

### Wrong execution order

**Issue**: Decorators execute in unexpected order

**Solution**: Remember attributes execute in **reverse order**. First attribute = outermost decorator.

## Best Practices

1. **Keep handlers focused** - One responsibility per handler
2. **Use Result pattern** - Avoid throwing exceptions for business logic failures
3. **Decorators should be composable** - Order shouldn't matter (when possible)
4. **Name handlers clearly** - Use descriptive names like `CreateVehicleCommandHandler`
5. **Add validation attributes** - Use DataAnnotations on command properties
6. **Don't mix concerns** - Keep business logic in handlers, cross-cutting concerns in decorators
7. **Test in isolation** - Unit test handlers and decorators separately

## Example: Full Create Vehicle Flow

1. **Controller receives request**
   ```
   POST /api/vehicle
   { "type": 1, "licensePlate": "ABC123" }
   ```

2. **Controller creates command and dispatches**
   ```csharp
   var command = new CreateVehicleCommand { Type = VehicleType.Car, LicensePlate = "ABC123" };
   var result = await _messages.Dispatch(command);
   ```

3. **Messages dispatcher resolves handler with decorators**
   ```
   LoggingDecorator → ValidationDecorator → TransactionDecorator → CreateVehicleCommandHandler
   ```

4. **Decorators execute in order**
   - LoggingDecorator: Logs "Executing command: CreateVehicleCommand"
   - ValidationDecorator: Validates LicensePlate (Required, StringLength)
   - TransactionDecorator: Prepares transaction context
   - Handler: Creates vehicle entity, calls repository
   - TransactionDecorator: Calls UnitOfWork.SaveChangesAsync()
   - LoggingDecorator: Logs "CreateVehicleCommand completed successfully"

5. **Controller returns response**
   ```csharp
   return Ok(new { message = "Vehicle created successfully" });
   ```

## Migration from Service Pattern

The decorator pipeline coexists with the existing service pattern. You can gradually migrate:

**Old Pattern** (still works):
```csharp
public class VehicleService : IVehicleService
{
    public async Task CreateVehicleAsync(VehicleDto dto)
    {
        // Business logic
    }
}
```

**New Pattern** (recommended):
```csharp
[Logging]
[Validation]
[Transaction]
public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand>
{
    public async Task<Result> Handle(CreateVehicleCommand command)
    {
        // Business logic
    }
}
```

Both patterns work side-by-side in the same codebase.
