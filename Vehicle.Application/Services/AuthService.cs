using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Infrastructure;
using Vehicle.Application.Models;
using Vehicle.Domain.Dtos;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Interfaces.Services;

namespace Vehicle.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPersonMasterRepository _personMasterRepository;
        private readonly Messages _messages;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IPersonMasterRepository personMasterRepository,
            Messages messages,
            IOptions<JwtSettings> jwtSettings)
        {
            _personMasterRepository = personMasterRepository;
            _messages = messages;
            _jwtSettings = jwtSettings.Value;
            
            if (string.IsNullOrEmpty(_jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured");
            }
        }

        public async Task<LoginResponse?> AuthenticateAsync(string username, string password)
        {
            var user = await _personMasterRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return null;
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!isPasswordValid)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new LoginResponse
            {
                Token = tokenString,
                PersonSyncId = user.PersonSyncId
            };
        }
    }
}
