using CoreAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using Vehicle.Application.Constants;
using Vehicle.Application.Dtos;
using Vehicle.Application.Models;
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
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var validationResponse = ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    ErrorCodes.VALIDATION_FAILED,
                    validationErrors
                );

                return BadRequest(validationResponse);
            }

            var token = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (token == null)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Invalid username or password",
                    ErrorCodes.AUTH_INVALID_CREDENTIALS
                );

                return Unauthorized(errorResponse);
            }

            var successResponse = ApiResponse<LoginResponse>.SuccessResponse(
                new LoginResponse { Token = token },
                "Login successful"
            );

            return Ok(successResponse);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var validationResponse = ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    ErrorCodes.VALIDATION_FAILED,
                    validationErrors
                );

                return BadRequest(validationResponse);
            }

            var token = await _authService.RegisterAsync(request.Username, request.Password);

            if (token == null)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Username is already taken",
                    ErrorCodes.AUTH_USERNAME_TAKEN
                );

                return BadRequest(errorResponse);
            }

            var successResponse = ApiResponse<RegisterResponse>.SuccessResponse(
                new RegisterResponse { Token = token },
                "Registration successful"
            );

            return Ok(successResponse);
        }
    }
}
