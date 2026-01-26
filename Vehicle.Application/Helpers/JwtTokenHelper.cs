using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Vehicle.Application.Models;

namespace Vehicle.Application.Helpers
{
    public static class JwtTokenHelper
    {
        /// <summary>
        /// Validates JWT token and extracts user ID
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="jwtSettings">JWT settings containing secret key, issuer, and audience</param>
        /// <param name="userId">Extracted user ID if valid</param>
        /// <returns>True if token is valid and not expired, false otherwise</returns>
        public static bool ValidateToken(string token, JwtSettings jwtSettings, out int userId)
        {
            userId = 0;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                // .NET automatically maps "sub" claim to ClaimTypes.NameIdentifier during validation
                var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);
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
    }
}
