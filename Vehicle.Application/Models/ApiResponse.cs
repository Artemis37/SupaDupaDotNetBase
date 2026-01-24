namespace Vehicle.Application.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                ErrorCode = null,
                Timestamp = DateTime.UtcNow,
                ValidationErrors = null
            };
        }

        public static ApiResponse<T> ErrorResponse(
            string message,
            string errorCode,
            Dictionary<string, string[]>? validationErrors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                ErrorCode = errorCode,
                Timestamp = DateTime.UtcNow,
                ValidationErrors = validationErrors
            };
        }
    }
}
