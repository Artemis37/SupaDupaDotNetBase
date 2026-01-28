using Moq;
using Shared.Application.Context;
using Vehicle.Application.Commands;
using Vehicle.Domain.Enums;
using Vehicle.Domain.Interfaces.Repositories;

namespace Vehicle.Application.Tests.Commands;

[TestFixture]
public class UpdateVehicleCommandHandlerTests
{
    private Mock<IVehicleRepository> _mockRepository;
    private UpdateVehicleCommandHandler _handler;
    private PersonContext _personContext;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IVehicleRepository>();
        _handler = new UpdateVehicleCommandHandler(_mockRepository.Object);
        
        _personContext = new PersonContext
        {
            PersonId = 1
        };
        PersonContextProvider.SetCurrent(_personContext);
    }

    [TearDown]
    public void TearDown()
    {
        PersonContextProvider.SetCurrent(null);
    }

    [Test]
    public async Task Handle_UpdatesVehicle_WhenVehicleExistsAndOwned()
    {
        var existingVehicle = new Domain.Models.Vehicle
        {
            Id = 1,
            PersonId = 1,
            Type = VehicleType.Car,
            LicensePlate = "OLD123",
            IsDeleted = false
        };

        var command = new UpdateVehicleCommand
        {
            Id = 1,
            Type = VehicleType.Motorcycle,
            LicensePlate = "NEW456"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingVehicle);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Domain.Models.Vehicle>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(command);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(existingVehicle.Type, Is.EqualTo(VehicleType.Motorcycle));
        Assert.That(existingVehicle.LicensePlate, Is.EqualTo("NEW456"));
        Assert.That(existingVehicle.UpdatedBy, Is.EqualTo(1));
        Assert.That(existingVehicle.UpdatedAt, Is.Not.Null);
        
        _mockRepository.Verify(r => r.UpdateAsync(existingVehicle), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task Handle_ReturnsFailure_WhenVehicleNotFound()
    {
        var command = new UpdateVehicleCommand
        {
            Id = 999,
            Type = VehicleType.Car,
            LicensePlate = "TEST123"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Domain.Models.Vehicle?)null);

        var result = await _handler.Handle(command);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Vehicle not found"));
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain.Models.Vehicle>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task Handle_ReturnsFailure_WhenVehicleNotOwned()
    {
        var existingVehicle = new Domain.Models.Vehicle
        {
            Id = 1,
            PersonId = 999,
            Type = VehicleType.Car,
            LicensePlate = "OTHER123",
            IsDeleted = false
        };

        var command = new UpdateVehicleCommand
        {
            Id = 1,
            Type = VehicleType.Car,
            LicensePlate = "TEST123"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingVehicle);

        var result = await _handler.Handle(command);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Unauthorized to update this vehicle"));
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain.Models.Vehicle>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task Handle_UpdatesAllProperties_Correctly()
    {
        var existingVehicle = new Domain.Models.Vehicle
        {
            Id = 1,
            PersonId = 1,
            Type = VehicleType.Car,
            LicensePlate = "OLD123",
            IsDeleted = false
        };

        var command = new UpdateVehicleCommand
        {
            Id = 1,
            Type = VehicleType.Motorcycle,
            LicensePlate = "NEW789"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingVehicle);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Domain.Models.Vehicle>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        await _handler.Handle(command);

        Assert.That(existingVehicle.Type, Is.EqualTo(VehicleType.Motorcycle));
        Assert.That(existingVehicle.LicensePlate, Is.EqualTo("NEW789"));
        Assert.That(existingVehicle.UpdatedBy, Is.EqualTo(_personContext.PersonId));
    }
}
