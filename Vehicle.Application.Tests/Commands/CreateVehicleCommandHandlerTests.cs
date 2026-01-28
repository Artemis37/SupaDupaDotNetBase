using Moq;
using Shared.Application.Context;
using Shared.Application.Models;
using Vehicle.Application.Commands;
using Vehicle.Domain.Enums;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Models;

namespace Vehicle.Application.Tests.Commands;

[TestFixture]
public class CreateVehicleCommandHandlerTests
{
    private Mock<IVehicleRepository> _mockRepository;
    private CreateVehicleCommandHandler _handler;
    private PersonContext _personContext;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IVehicleRepository>();
        _handler = new CreateVehicleCommandHandler(_mockRepository.Object);
        
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
    public async Task Handle_CreatesVehicle_WithCorrectProperties()
    {
        var command = new CreateVehicleCommand
        {
            Type = VehicleType.Car,
            LicensePlate = "ABC123"
        };

        Domain.Models.Vehicle capturedVehicle = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Models.Vehicle>()))
            .Callback<Domain.Models.Vehicle>(v => capturedVehicle = v)
            .ReturnsAsync((Domain.Models.Vehicle v) => v);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(command);

        Assert.That(result.IsSuccess, Is.True);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Domain.Models.Vehicle>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        
        Assert.That(capturedVehicle, Is.Not.Null);
        Assert.That(capturedVehicle.Type, Is.EqualTo(VehicleType.Car));
        Assert.That(capturedVehicle.LicensePlate, Is.EqualTo("ABC123"));
        Assert.That(capturedVehicle.PersonId, Is.EqualTo(1));
        Assert.That(capturedVehicle.CreatedBy, Is.EqualTo(1));
        Assert.That(capturedVehicle.IsDeleted, Is.False);
        Assert.That(capturedVehicle.CreatedAt, Is.Not.Null);
    }

    [Test]
    public async Task Handle_CreatesVehicle_WithMotorcycleType()
    {
        var command = new CreateVehicleCommand
        {
            Type = VehicleType.Motorcycle,
            LicensePlate = "XYZ789"
        };

        Domain.Models.Vehicle capturedVehicle = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Models.Vehicle>()))
            .Callback<Domain.Models.Vehicle>(v => capturedVehicle = v)
            .ReturnsAsync((Domain.Models.Vehicle v) => v);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(command);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(capturedVehicle.Type, Is.EqualTo(VehicleType.Motorcycle));
        Assert.That(capturedVehicle.LicensePlate, Is.EqualTo("XYZ789"));
    }

    [Test]
    public async Task Handle_SetsPersonId_FromContext()
    {
        var command = new CreateVehicleCommand
        {
            Type = VehicleType.Car,
            LicensePlate = "TEST123"
        };

        Domain.Models.Vehicle capturedVehicle = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Models.Vehicle>()))
            .Callback<Domain.Models.Vehicle>(v => capturedVehicle = v)
            .ReturnsAsync((Domain.Models.Vehicle v) => v);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        await _handler.Handle(command);

        Assert.That(capturedVehicle.PersonId, Is.EqualTo(_personContext.PersonId));
        Assert.That(capturedVehicle.CreatedBy, Is.EqualTo(_personContext.PersonId));
    }
}
