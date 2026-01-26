using Vehicle.Domain.Dtos;

namespace Vehicle.Domain.Interfaces.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plain text password</param>
        /// <returns>LoginResponse with token and personSyncId if authentication successful, null otherwise</returns>
        Task<LoginResponse?> AuthenticateAsync(string username, string password);
    }
}
