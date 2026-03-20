using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Helpers;
using IdentityService.Models;
using IdentityService.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MassTransit;
using SmartSure.Shared.Contracts.Events;

namespace IdentityService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IBus _bus;

        public AuthService(IUserRepository repo, TokenService tokenService, IConfiguration config, IMemoryCache cache, IBus bus)
        {
            _repo = repo;
            _tokenService = tokenService;
            _config = config;
            _cache = cache;
            _bus = bus;
        }

        public async Task ChangePassword(string userId, ChangePasswordDTO dto)
        {
            var user = await _repo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) throw new Exception("User not found");

            if (!PasswordHasher.Verify(dto.OldPassword, user.Password.PasswordHash))
                throw new Exception("Invalid old password");

            user.Password.PasswordHash = PasswordHasher.PasswordHash(dto.NewPassword);
            await _repo.SaveChangesAsync();
        }

        public async Task<UserDTO> GetProfile(string userId)
        {
            var user = await _repo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) throw new Exception("User not found");

            return new UserDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName
            };
        }

        public async Task<TokenResponseDTO> Login(LoginDTO dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Invalid credentials");

            if (!PasswordHasher.Verify(dto.Password, user.Password.PasswordHash))
                throw new Exception("Invalid credentials");

            var roles = user.UserRoles?.Select(ur => ur.Role.RoleName).ToList() ?? new List<string>();
            var audience = new List<string> { _config["Jwt:Audience"] ?? _config["Jwt:Issuer"] };
            
            var token = _tokenService.BuildToken(_config["Jwt:Key"], _config["Jwt:Issuer"], audience, user.UserId.ToString(), roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Cache refresh token for 24 hours (No DB storage as requested)
            _cache.Set($"refreshToken_{refreshToken}", user.UserId.ToString(), TimeSpan.FromHours(24));

            return new TokenResponseDTO 
            { 
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponseDTO> Refresh(string refreshToken)
        {
            if (!_cache.TryGetValue($"refreshToken_{refreshToken}", out string userIdStr))
            {
                throw new Exception("Invalid or expired refresh token");
            }

            var userId = Guid.Parse(userIdStr);
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            var roles = user.UserRoles?.Select(ur => ur.Role.RoleName).ToList() ?? new List<string>();
            var audience = new List<string> { _config["Jwt:Audience"] ?? _config["Jwt:Issuer"] };

            var newToken = _tokenService.BuildToken(_config["Jwt:Key"], _config["Jwt:Issuer"], audience, user.UserId.ToString(), roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old token and cache new one
            _cache.Remove($"refreshToken_{refreshToken}");
            _cache.Set($"refreshToken_{newRefreshToken}", user.UserId.ToString(), TimeSpan.FromHours(24));

            return new TokenResponseDTO
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<string> Register(RegisterDTO dto)
        {
            var existingUser = await _repo.GetByEmailAsync(dto.Email);
            if (existingUser != null) throw new Exception("Email already exists");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = "", 
                Password = new Password
                {
                    PassId = Guid.NewGuid(),
                    PasswordHash = PasswordHasher.PasswordHash(dto.Password)
                }
            };
            user.Password.UserId = user.UserId;

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            await _bus.Publish(new UserRegisteredEvent(
                user.UserId,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                DateTime.UtcNow,
                false
            ));

            return "Registration successful";
        }

        public async Task UpdateProfile(string userId, UpdateUserDTO dto)
        {
            var user = await _repo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) throw new Exception("User not found");

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Address = dto.Address;

            await _repo.SaveChangesAsync();
        }
    }
}
