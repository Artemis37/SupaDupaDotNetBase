using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.Json;
using Vehicle.Application.Constants;
using Vehicle.Application.Exceptions;
using Vehicle.Application.Models;

namespace CoreAPI.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, errorCode, message) = MapExceptionToResponse(exception);

            _logger.LogError(exception, "An exception occurred: {Message}. ErrorCode: {ErrorCode}, StatusCode: {StatusCode}",
                exception.Message, errorCode, statusCode);

            var response = ApiResponse<object>.ErrorResponse(message, errorCode);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(json, Encoding.UTF8);
        }

        private (int statusCode, string errorCode, string message) MapExceptionToResponse(Exception exception)
        {
            return exception switch
            {
                BusinessLogicException => (
                    StatusCodes.Status409Conflict,
                    ErrorCodes.BUSINESS_LOGIC_ERROR,
                    exception.Message
                ),
                SqlException => (
                    StatusCodes.Status500InternalServerError,
                    ErrorCodes.DATABASE_ERROR,
                    "Database operation failed"
                ),
                NullReferenceException => (
                    StatusCodes.Status500InternalServerError,
                    ErrorCodes.NULL_REFERENCE_ERROR,
                    "Null reference error occurred"
                ),
                IndexOutOfRangeException => (
                    StatusCodes.Status500InternalServerError,
                    ErrorCodes.INDEX_OUT_OF_RANGE_ERROR,
                    "Index out of range error occurred"
                ),
                ArgumentException or ArgumentNullException => (
                    StatusCodes.Status400BadRequest,
                    ErrorCodes.INVALID_ARGUMENT,
                    exception is ArgumentNullException ? "Required argument is null" : "Invalid argument provided"
                ),
                InvalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    ErrorCodes.INVALID_OPERATION,
                    "Invalid operation"
                ),
                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    ErrorCodes.ENTITY_NOT_FOUND,
                    "Requested entity not found"
                ),
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    ErrorCodes.UNAUTHORIZED_ACCESS,
                    "Unauthorized access"
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    ErrorCodes.INTERNAL_SERVER_ERROR,
                    "An unexpected error occurred"
                )
            };
        }
    }
}
