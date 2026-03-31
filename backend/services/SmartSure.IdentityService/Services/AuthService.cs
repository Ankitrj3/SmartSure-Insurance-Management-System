using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Helpers;
using IdentityService.Models;
using IdentityService.Repositories;
using Microsoft.Extensions.Caching.Memory;
using MassTransit;
using SmartSure.Shared.Contracts.Events;
using SmartSure.Shared.Contracts.Exceptions;

namespace IdentityService.Services
{
    /// <summary>
    /// Core service responsible for user authentication, registration workflows,
    /// password management, and JWT generation. Highlights standard Identity procedures.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IBus _bus;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;

        public AuthService(IUserRepository repo, TokenService tokenService, IConfiguration config,
            IMemoryCache cache, IBus bus, IEmailService emailService, IOtpService otpService)
        {
            _repo = repo;
            _tokenService = tokenService;
            _config = config;
            _cache = cache;
            _bus = bus;
            _emailService = emailService;
            _otpService = otpService;
        }

        #region User Profile & Management

        /// <summary>
        /// Allows a user to change their existing password by verifying the old password first.
        /// </summary>
        public async Task ChangePassword(string userId, ChangePasswordDTO dto)
        {
            var user = await _repo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) throw new NotFoundException("User");

            if (!PasswordHasher.Verify(dto.OldPassword, user.Password.PasswordHash))
                throw new ValidationException("Invalid old password.");

            user.Password.PasswordHash = PasswordHasher.PasswordHash(dto.NewPassword);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Resets a user's password using a one-time password (OTP) verification safely.
        /// </summary>
        public async Task ResetPasswordAsync(ResetPasswordWithOtpDTO dto)
        {
            bool isValid = await _otpService.ValidateOtpAsync(dto.Email, dto.Otp);
            if (!isValid) throw new ValidationException("Invalid or expired OTP.");

            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null) throw new NotFoundException("User");

            user.Password.PasswordHash = PasswordHasher.PasswordHash(dto.NewPassword);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the profile information for a specific user ID.
        /// </summary>
        public async Task<UserDTO> GetProfile(string userId)
        {
            var user = await _repo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) throw new NotFoundException("User");

            return new UserDTO
            {
                UserId      = user.UserId,
                Email       = user.Email,
                FullName    = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address     = user.Address
            };
        }

        /// <summary>
        /// Performs the GetAllUsers operation.
        /// </summary>
        public async Task<List<UserDTO>> GetAllUsers()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(user => new UserDTO
            {
                UserId      = user.UserId,
                Email       = user.Email,
                FullName    = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address     = user.Address
            }).ToList();
        }

        #endregion

        #region Authentication & Sessions

        /// <summary>
        /// Authenticates user credentials and issues a JWT token along with a refresh token.
        /// </summary>
        public async Task<TokenResponseDTO> Login(LoginDTO dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null) throw new UnauthorizedException("Invalid credentials.");

            if (!PasswordHasher.Verify(dto.Password, user.Password.PasswordHash))
                throw new UnauthorizedException("Invalid credentials.");

            var roles    = user.UserRoles?.Select(ur => ur.Role.RoleName).ToList() ?? new List<string>();
            var audience = _config["Jwt:Audience"] ?? _config["Jwt:Issuer"]!;

            var token        = _tokenService.BuildToken(_config["Jwt:Key"], _config["Jwt:Issuer"], audience, user.UserId.ToString(), roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Cache refresh token for 24 hours (no DB storage)
            _cache.Set($"refreshToken_{refreshToken}", user.UserId.ToString(), TimeSpan.FromHours(24));

            return new TokenResponseDTO
            {
                Token        = token,
                RefreshToken = refreshToken,
                Role         = roles.FirstOrDefault() ?? "Customer"
            };
        }

        /// <summary>
        /// Refreshes a JWT using a valid, cached refresh token to prolong user sessions transparently.
        /// </summary>
        public async Task<TokenResponseDTO> Refresh(string refreshToken)
        {
            if (!_cache.TryGetValue($"refreshToken_{refreshToken}", out string? userIdStr))
                throw new UnauthorizedException("Invalid or expired refresh token.");

            var userId = Guid.Parse(userIdStr!);
            var user   = await _repo.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User");

            var roles    = user.UserRoles?.Select(ur => ur.Role.RoleName).ToList() ?? new List<string>();
            var audience = _config["Jwt:Audience"] ?? _config["Jwt:Issuer"]!;

            var newToken        = _tokenService.BuildToken(_config["Jwt:Key"], _config["Jwt:Issuer"], audience, user.UserId.ToString(), roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old token and cache new one
            _cache.Remove($"refreshToken_{refreshToken}");
            _cache.Set($"refreshToken_{newRefreshToken}", user.UserId.ToString(), TimeSpan.FromHours(24));

            return new TokenResponseDTO
            {
                Token        = newToken,
                RefreshToken = newRefreshToken,
                Role         = roles.FirstOrDefault() ?? "Customer"
            };
        }

        #endregion

        #region Registration Pipeline

        /// <summary>
        /// Initiates user registration by validating email uniqueness and triggering an OTP workflow.
        /// Registration data is kept in memory cache until verified.
        /// </summary>
        public async Task<string> Register(RegisterDTO dto)
        {
            var existingUser = await _repo.GetByEmailAsync(dto.Email);
            if (existingUser != null) throw new ConflictException("Email already exists.");

            // Generate a 6-digit OTP
            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            // Cache the OTP and DTO
            _cache.Set($"RegistrationOtp_{dto.Email}",  otp, TimeSpan.FromMinutes(10));
            _cache.Set($"RegistrationData_{dto.Email}", dto, TimeSpan.FromMinutes(10));

            // Send Email
            string subject = "SmartSure - Verify Your Registration";
            string body    = $"Hello {dto.FullName},<br/><br/>Your 6-digit verification code is: <b>{otp}</b>.<br/>This code will expire in 10 minutes.";
            await _emailService.SendEmailAsync(dto.Email, subject, body);

            return "OTP sent successfully. Please check your email to verify and complete registration.";
        }

        /// <summary>
        /// Completes user registration by validating the supplied OTP. 
        /// Creates the user and broadcast the 'UserRegisteredEvent' to other microservices.
        /// </summary>
        public async Task<string> VerifyRegistrationOtp(VerifyOtpDTO dto)
        {
            if (!_cache.TryGetValue($"RegistrationOtp_{dto.Email}", out string? cachedOtp))
                throw new ValidationException("OTP expired or invalid.");

            if (cachedOtp != dto.Otp)
                throw new ValidationException("Incorrect OTP.");

            if (!_cache.TryGetValue($"RegistrationData_{dto.Email}", out RegisterDTO? regDto))
                throw new ValidationException("Registration data not found or expired. Please register again.");

            // Create user
            var user = new User
            {
                UserId          = Guid.NewGuid(),
                FullName        = regDto!.FullName,
                Email           = regDto.Email,
                PhoneNumber     = regDto.PhoneNumber,
                Address         = "",
                IsEmailVerified = true,
                Password = new Password
                {
                    PassId       = Guid.NewGuid(),
                    PasswordHash = PasswordHasher.PasswordHash(regDto.Password)
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

            // Remove from cache
            _cache.Remove($"RegistrationOtp_{dto.Email}");
            _cache.Remove($"RegistrationData_{dto.Email}");

            // Send Welcome Email
            string subject = "Welcome to SmartSure – Registration Successful";
            string body    = $"Hello {user.FullName},<br/><br/>Your registration was successful! Welcome to SmartSure Insurance.<br/>You can now log in and start using our services.";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return "Registration successful and verified";
        }

        /// <summary>
        /// Performs the UpdateProfile operation.
        /// </summary>
        public async Task UpdateProfile(string userId, UpdateUserDTO dto)
        {
            var user = await _repo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) throw new NotFoundException("User");

            user.FullName    = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Address     = dto.Address;

            await _repo.SaveChangesAsync();
        }

        #endregion
    }
}
