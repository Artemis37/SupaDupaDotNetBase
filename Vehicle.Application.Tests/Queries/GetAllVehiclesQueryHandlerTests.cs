using Moq;
using Vehicle.Application.Queries;
using Vehicle.Domain.Enums;
using Vehicle.Domain.Interfaces.Repositories;

namespace Vehicle.Application.Tests.Queries;

[TestFixture]
public class GetAllVehiclesQueryHandlerTests
{
    private Mock<IVehicleRepository> _mockRepository;
    private GetAllVehiclesQueryHandler _handler;
    private List<Domain.Models.Vehicle> _testVehicles;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IVehicleRepository>();
        _handler = new GetAllVehiclesQueryHandler(_mockRepository.Object);
        
        _testVehicles = new List<Domain.Models.Vehicle>
        {
            new Domain.Models.Vehicle
            {
                Id = 1,
                PersonId = 1,
                Type = VehicleType.Car,
                LicensePlate = "ABC123",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            },
            new Domain.Models.Vehicle
            {
                Id = 2,
                PersonId = 1,
                Type = VehicleType.Motorcycle,
                LicensePlate = "XYZ789",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false
            },
            new Domain.Models.Vehicle
            {
                Id = 3,
                PersonId = 1,
                Type = VehicleType.Car,
                LicensePlate = "DEF456",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false
            }
        };
    }

    [Test]
    public async Task Handle_ReturnsAllVehicles_WhenNoSearchText()
    {
        var query = new GetAllVehiclesQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetPagedVehiclesAsync(null, 1, 10))
            .ReturnsAsync((_testVehicles, 3));

        var result = await _handler.Handle(query);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(3));
        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
    }

    [Test]
    public async Task Handle_ReturnsFilteredVehicles_WhenSearchTextProvided()
    {
        var query = new GetAllVehiclesQuery
        {
            SearchText = "abc",
            PageNumber = 1,
            PageSize = 10
        };

        var filteredVehicles = _testVehicles.Where(v => v.LicensePlate.ToLower().Contains("abc")).ToList();
        _mockRepository.Setup(r => r.GetPagedVehiclesAsync("abc", 1, 10))
            .ReturnsAsync((filteredVehicles, 1));

        var result = await _handler.Handle(query);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(1));
        Assert.That(result.Items[0].LicensePlate, Is.EqualTo("ABC123"));
        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Handle_ReturnsFilteredVehicles_CaseInsensitive()
    {
        var query = new GetAllVehiclesQuery
        {
            SearchText = "XYZ",
            PageNumber = 1,
            PageSize = 10
        };

        var filteredVehicles = _testVehicles.Where(v => v.LicensePlate.ToLower().Contains("xyz")).ToList();
        _mockRepository.Setup(r => r.GetPagedVehiclesAsync("XYZ", 1, 10))
            .ReturnsAsync((filteredVehicles, 1));

        var result = await _handler.Handle(query);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(1));
        Assert.That(result.Items[0].LicensePlate, Is.EqualTo("XYZ789"));
    }

    [Test]
    public async Task Handle_ReturnsPagedResults_WhenPaginationApplied()
    {
        var query = new GetAllVehiclesQuery
        {
            PageNumber = 2,
            PageSize = 2
        };

        var pagedVehicles = _testVehicles.Skip(2).Take(2).ToList();
        _mockRepository.Setup(r => r.GetPagedVehiclesAsync(null, 2, 2))
            .ReturnsAsync((pagedVehicles, 3));

        var result = await _handler.Handle(query);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(1));
        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.PageNumber, Is.EqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_ReturnsEmptyList_WhenNoVehiclesMatch()
    {
        var query = new GetAllVehiclesQuery
        {
            SearchText = "NONEXISTENT",
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetPagedVehiclesAsync("NONEXISTENT", 1, 10))
            .ReturnsAsync((new List<Domain.Models.Vehicle>(), 0));

        var result = await _handler.Handle(query);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(0));
        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Handle_ReturnsCorrectVehicleDto_WithAllProperties()
    {
        var query = new GetAllVehiclesQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetPagedVehiclesAsync(null, 1, 10))
            .ReturnsAsync((_testVehicles, 3));

        var result = await _handler.Handle(query);

        Assert.That(result.Items[0].Id, Is.EqualTo(1));
        Assert.That(result.Items[0].PersonId, Is.EqualTo(1));
        Assert.That(result.Items[0].Type, Is.EqualTo(VehicleType.Car));
        Assert.That(result.Items[0].LicensePlate, Is.EqualTo("ABC123"));
        Assert.That(result.Items[0].CreatedAt, Is.Not.Null);
    }
}
