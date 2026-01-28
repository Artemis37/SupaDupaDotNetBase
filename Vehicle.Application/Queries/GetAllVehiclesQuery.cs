using Shared.Application.Context;
using Shared.Application.Interfaces;
using Vehicle.Application.Converters;
using Vehicle.Application.Decorators;
using Vehicle.Application.Dtos;
using Vehicle.Domain.Interfaces.Repositories;

namespace Vehicle.Application.Queries;

public class GetAllVehiclesQuery : IQuery<PagedResult<VehicleDto>>
{
    public string? SearchText { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

[LoggingQuery]
public class GetAllVehiclesQueryHandler : IQueryHandler<GetAllVehiclesQuery, PagedResult<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetAllVehiclesQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<PagedResult<VehicleDto>> Handle(GetAllVehiclesQuery query)
    {
        var (vehicles, totalCount) = await _vehicleRepository.GetPagedVehiclesAsync(
            query.SearchText,
            query.PageNumber,
            query.PageSize);

        var items = DomainConverter.Map(vehicles);

        return new PagedResult<VehicleDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
