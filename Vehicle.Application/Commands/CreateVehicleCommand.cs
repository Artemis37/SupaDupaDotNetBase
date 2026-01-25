using System.ComponentModel.DataAnnotations;
using Shared.Application.Context;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Vehicle.Application.Decorators;
using Vehicle.Domain.Enums;
using Vehicle.Domain.Interfaces.Repositories;
using ValidationAttribute = Vehicle.Application.Decorators.ValidationAttribute;

namespace Vehicle.Application.Commands;

public class CreateVehicleCommand : ICommand
{
    public VehicleType Type { get; set; }
    
    [Required(ErrorMessage = "License plate is required")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "License plate must be between 1 and 20 characters")]
    public string LicensePlate { get; set; } = string.Empty;
}

[LoggingCommand]
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
        // Create vehicle
        var vehicle = new Domain.Models.Vehicle
        {
            PersonId = _personContext.PersonId ?? 0,
            Type = command.Type,
            LicensePlate = command.LicensePlate,
            CreatedBy = _personContext.PersonId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var createdVehicle = await _vehicleRepository.AddAsync(vehicle);
        
        // Return success with vehicle ID
        return Result.Ok();
    }
}
