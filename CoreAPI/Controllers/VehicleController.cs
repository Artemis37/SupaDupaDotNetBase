using Microsoft.AspNetCore.Mvc;
using Shared.Application.Context;
using Shared.Application.Infrastructure;
using Vehicle.Application.Commands;
using Vehicle.Application.Dtos;
using Vehicle.Application.Models;
using Vehicle.Application.Queries;

namespace CoreAPI.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? searchText, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAllVehiclesQuery
        {
            SearchText = searchText,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _messages.Dispatch(query);

        var response = ApiResponse<PagedResult<VehicleDto>>.SuccessResponse(result, "Vehicles retrieved successfully");
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateVehicleRequest request)
    {
        var command = new UpdateVehicleCommand
        {
            Id = id,
            Type = request.Type,
            LicensePlate = request.LicensePlate
        };

        var result = await _messages.Dispatch(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Vehicle updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var command = new DeleteVehicleCommand
        {
            Id = id
        };

        var result = await _messages.Dispatch(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Vehicle deleted successfully" });
    }
}
