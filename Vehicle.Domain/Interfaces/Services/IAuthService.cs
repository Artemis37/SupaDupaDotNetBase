namespace Vehicle.Domain.Interfaces.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plain text password</param>
        /// <returns>JWT token if authentication successful, null otherwise</returns>
        Task<string?> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Validates JWT token and extracts user ID
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="userId">Extracted user ID if valid</param>
        /// <returns>True if token is valid and not expired, false otherwise</returns>
        bool ValidateToken(string token, out int userId);
    }
}
