using System.ComponentModel.DataAnnotations;
using Shared.Application.Context;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Vehicle.Application.Decorators;
using Vehicle.Domain.Enums;
using Vehicle.Domain.Interfaces.Repositories;
using ValidationAttribute = Vehicle.Application.Decorators.ValidationAttribute;

namespace Vehicle.Application.Commands;

public class UpdateVehicleCommand : ICommand
{
    public int Id { get; set; }
    public VehicleType Type { get; set; }
    
    [Required(ErrorMessage = "License plate is required")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "License plate must be between 1 and 20 characters")]
    public string LicensePlate { get; set; } = string.Empty;
}

[LoggingCommand]
[Validation]
public class UpdateVehicleCommandHandler : ICommandHandler<UpdateVehicleCommand>
{
    private readonly IVehicleRepository _vehicleRepository;

    public UpdateVehicleCommandHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result> Handle(UpdateVehicleCommand command)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.Id);
        
        if (vehicle == null)
        {
            return Result.Fail("Vehicle not found");
        }

        var currentPersonId = PersonContextProvider.Current?.PersonId;
        if (vehicle.PersonId != currentPersonId)
        {
            return Result.Fail("Unauthorized to update this vehicle");
        }

        vehicle.Type = command.Type;
        vehicle.LicensePlate = command.LicensePlate;
        vehicle.UpdatedBy = currentPersonId;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _vehicleRepository.UpdateAsync(vehicle);
        await _vehicleRepository.SaveChangesAsync();
        
        return Result.Ok();
    }
}
