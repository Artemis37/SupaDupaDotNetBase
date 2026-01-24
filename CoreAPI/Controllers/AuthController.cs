using CoreAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using Vehicle.Application.Dtos;
using Vehicle.Domain.Interfaces.Services;

namespace CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SkipAuthentication]
    [SkipPersonIdCheck]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (token == null)
            {
                return Unauthorized(new ErrorResponse { Error = "Invalid username or password" });
            }

            return Ok(new LoginResponse { Token = token });
        }
    }
}
