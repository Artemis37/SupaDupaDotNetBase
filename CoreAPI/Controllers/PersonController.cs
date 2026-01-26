using CoreAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Infrastructure;
using Vehicle.Application.Commands;
using Vehicle.Application.Constants;
using Vehicle.Application.Models;

namespace CoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[SkipAuthentication]
[SkipPersonIdCheck]
public class PersonController : ControllerBase
{
    private readonly Messages _messages;

    public PersonController(Messages messages)
    {
        _messages = messages;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePersonCommand command)
    {
        var result = await _messages.Dispatch(command);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Error,
                ErrorCodes.BUSINESS_LOGIC_ERROR
            );
            return BadRequest(errorResponse);
        }

        var successResponse = ApiResponse<object>.SuccessResponse(
            true,
            "Person created successfully"
        );

        return Ok(successResponse);
    }
}
