using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Helpers
{
    /// <summary>
    /// Represent or implements TokenService.
    /// </summary>
    public class TokenService
    {
        private TimeSpan ExpiryDuration = new TimeSpan(20, 30, 0);

        /// <summary>
        /// Performs the BuildToken operation.
        /// </summary>
        public string BuildToken(string key, string issuer, string audience, string userName, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.UniqueName,userName)
            };

            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience ?? issuer,
                claims: claims,
                expires: DateTime.Now.Add(ExpiryDuration),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        /// <summary>
        /// Performs the GenerateRefreshToken operation.
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
