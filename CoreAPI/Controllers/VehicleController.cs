using Microsoft.AspNetCore.Mvc;
using Shared.Application.Context;
using Shared.Application.Infrastructure;
using Vehicle.Application.Commands;
using Vehicle.Application.Dtos;
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
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllVehiclesQuery();
        var result = await _messages.Dispatch(query);

        return Ok(result);
    }
}
