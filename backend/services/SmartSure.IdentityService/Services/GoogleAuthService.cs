using System.Net.Http.Headers;
using System.Text.Json;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SmartSure.Shared.Contracts.Events;

namespace IdentityService.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _config;
        private readonly IdentityDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IBus _bus;
        private readonly HttpClient _httpClient;

        public GoogleAuthService(IConfiguration config, IdentityDbContext context, TokenService tokenService, IBus bus, HttpClient httpClient)
        {
            _config = config;
            _context = context;
            _tokenService = tokenService;
            _bus = bus;
            _httpClient = httpClient;
        }

        public string GetGoogleLoginUrl()
        {
            var clientId = _config["Google:ClientId"];
            var redirectUri = _config["Google:RedirectUri"];
            return $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=email profile&access_type=offline";
        }

        public async Task<string> ProcessGoogleCallbackAsync(string code)
        {
            var clientId = _config["Google:ClientId"];
            var clientSecret = _config["Google:ClientSecret"];
            var redirectUri = _config["Google:RedirectUri"];

            var values = new Dictionary<string, string>
            {
                { "client_id", clientId! },
                { "client_secret", clientSecret! },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUri! }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Google token exchange failed");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var idToken = doc.RootElement.GetProperty("id_token").GetString()!;

            // Verify Google token roughly
            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(idToken);
            var payload = jwt.Payload;

            var email = payload["email"].ToString()!;
            var name = payload.ContainsKey("name") ? payload["name"].ToString()! : email;
            var googleSubjectId = payload["sub"].ToString()!;

            // Check if user exists
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = email,
                    FullName = name,
                    PhoneNumber = "Not Provided",
                    Address = "Not Provided",
                    IsGoogleAuth = true,
                    IsEmailVerified = true,
                    Password = new Password { PassId = Guid.NewGuid(), PasswordHash = "" } // No password for Google users
                };
                user.Password.UserId = user.UserId;

                // Assign default role (Customer)
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
                if (defaultRole != null)
                {
                    user.UserRoles = new List<UserRole>
                    {
                        new UserRole { UserId = user.UserId, RoleId = defaultRole.RoleId, Role = defaultRole }
                    };
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Publish Event
                await _bus.Publish(new UserRegisteredEvent(user.UserId, user.Email, user.FullName, "", DateTime.UtcNow, true));
            }

            // Map standard roles
            var roles = user.UserRoles?.Where(ur => ur.Role != null).Select(ur => ur.Role.RoleName).ToList() ?? new List<string> { "Customer" };
            if (!roles.Any()) roles.Add("Customer");
            var audience = new List<string> { _config["Jwt:Audience"] ?? _config["Jwt:Issuer"] };

            // Generate our own System Token
            return _tokenService.BuildToken(_config["Jwt:Key"], _config["Jwt:Issuer"], audience, user.UserId.ToString(), roles);
        }
    }
}
