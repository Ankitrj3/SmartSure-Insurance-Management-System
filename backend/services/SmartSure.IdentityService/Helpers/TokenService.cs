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
        public virtual string BuildToken(string key, string issuer, IEnumerable<string> audiences, string userName, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.UniqueName,userName)
            };

            if (audiences != null && audiences.Any())
            {
                foreach (var aud in audiences)
                {
                    claims.Add(new Claim(JwtRegisteredClaimNames.Aud, aud));
                }
            }

            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: null,
                claims: claims,
                expires: DateTime.Now.Add(ExpiryDuration),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        /// <summary>
        /// Performs the GenerateRefreshToken operation.
        /// </summary>
        public virtual string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
