using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Transactions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Context;
using Vehicle.Application.Models;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Interfaces.Services;
using Vehicle.Domain.Models;

namespace Vehicle.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPersonMasterRepository _personMasterRepository;
        private readonly IPersonRepository _personRepository;
        private readonly PersonContext _personContext;
        private readonly ShardingSettings _shardingSettings;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IPersonMasterRepository personMasterRepository,
            IPersonRepository personRepository,
            PersonContext personContext,
            IOptions<ShardingSettings> shardingSettings,
            IOptions<JwtSettings> jwtSettings)
        {
            _personMasterRepository = personMasterRepository;
            _personRepository = personRepository;
            _personContext = personContext;
            _shardingSettings = shardingSettings.Value;
            _jwtSettings = jwtSettings.Value;
            
            if (string.IsNullOrEmpty(_jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured");
            }
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
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
            return tokenHandler.WriteToken(token);
        }

        public async Task<string?> RegisterAsync(string username, string password)
        {
            var existingUser = await _personMasterRepository.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                return null;
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            int shardId;
            if (_shardingSettings.HotShard.HasValue && 
                _shardingSettings.HotShard.Value >= 1 && 
                _shardingSettings.HotShard.Value <= _shardingSettings.TotalShards)
            {
                shardId = _shardingSettings.HotShard.Value;
            }
            else
            {
                shardId = Random.Shared.Next(1, _shardingSettings.TotalShards + 1);
            }

            // TODO: Replace direct database call with message broker for eventual consistency
            // Potential optimizations:
            // 1. SAGA Pattern: Implement compensating transactions to rollback Master DB 
            //    if Sharding DB creation fails (e.g., delete PersonMaster if Person creation fails)
            // 2. Outbox Pattern with Idempotency: Store "PersonCreated" event in Master DB outbox table,
            //    process via background worker to create Person in Sharding DB, ensuring at-least-once
            //    delivery with idempotent handlers to prevent duplicate Person records
            // 3. Event Sourcing: Store registration as immutable event, rebuild state by replaying events
            // 4. Two-Phase Message: Publish "PersonCreating" (prepare), then "PersonCreated" (commit) 
            //    or "PersonCreationFailed" (abort) events to message broker
            // 5. Async Creation: Return success immediately after Master DB write, create Sharding DB 
            //    record asynchronously via message queue (eventual consistency trade-off)
            // Current implementation: TransactionScope with 2PC for ACID guarantees across databases

            // Use TransactionScope for distributed transaction (2-phase commit)
            // This only work reliablely because both database are sql server, so TransactionScope automatically escalate to MSDTC
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Create PersonMaster in Master DB
                var personMaster = new PersonMaster
                {
                    Username = username,
                    Password = hashedPassword,
                    ShardId = shardId,
                    CreatedAt = DateTime.UtcNow
                };

                personMaster = await _personMasterRepository.AddAsync(personMaster);

                // Set PersonContext to route to the correct shard
                // This only work because of this is the first time _personRepository.AddAsync is called within the request
                // So new ShardingDbContext is created, might need a better solution to switch sharding in run time
                _personContext.PersonId = personMaster.Id;
                _personContext.ShardId = personMaster.ShardId;

                var person = new Person
                {
                    Name = username,
                    CreatedBy = personMaster.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _personRepository.AddAsync(person);

                scope.Complete();
            }

            var user = await _personMasterRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return null;
            }

            return GenerateJwtToken(user.Id);
        }

        public bool ValidateToken(string token, out int userId)
        {
            userId = 0;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
                if (subClaim == null || !int.TryParse(subClaim.Value, out userId))
                {
                    return false;
                }

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GenerateJwtToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
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
            return tokenHandler.WriteToken(token);
        }
    }
}
