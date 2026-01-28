using Shared.Application.Context;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Vehicle.Application.Decorators;
using Vehicle.Domain.Interfaces.Repositories;

namespace Vehicle.Application.Commands;

public class DeleteVehicleCommand : ICommand
{
    public int Id { get; set; }
}

[LoggingCommand]
public class DeleteVehicleCommandHandler : ICommandHandler<DeleteVehicleCommand>
{
    private readonly IVehicleRepository _vehicleRepository;

    public DeleteVehicleCommandHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result> Handle(DeleteVehicleCommand command)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.Id);
        
        if (vehicle == null)
        {
            return Result.Fail("Vehicle not found");
        }

        var currentPersonId = PersonContextProvider.Current?.PersonId;
        if (vehicle.PersonId != currentPersonId)
        {
            return Result.Fail("Unauthorized to delete this vehicle");
        }

        vehicle.IsDeleted = true;
        vehicle.UpdatedBy = currentPersonId;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _vehicleRepository.UpdateAsync(vehicle);
        await _vehicleRepository.SaveChangesAsync();
        
        return Result.Ok();
    }
}
