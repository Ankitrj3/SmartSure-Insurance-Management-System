using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartSure.Shared.Contracts.Exceptions;

namespace IdentityService.Controllers
{
    /// <summary>
    /// Manages Authentication endpoints including login, registration, passwords, and external auth providers.
    /// Exposes secure methods to issue and validate JWTs token.
    /// </summary>
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IOtpService _otpService;

        public AuthController(IAuthService authService, IGoogleAuthService googleAuthService, IOtpService otpService)
        {
            _authService = authService;
            _googleAuthService = googleAuthService;
            _otpService = otpService;
        }

        #region Google Authentication

        /// <summary>
        /// Retrieves the Google OAuth redirect URL and forwards the client.
        /// </summary>
        [HttpGet("google")]
        public IActionResult GoogleLogin()
        {
            var url = _googleAuthService.GetGoogleLoginUrl();
            return Redirect(url);
        }

        /// <summary>
        /// Callback handler for the Google OAuth authorization flow. Generates JWT internally.
        /// </summary>
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code)
        {
            try
            {
                var token = await _googleAuthService.ProcessGoogleCallbackAsync(code);
                // Return a redirect to the Angular frontend completely formatted with the valid JWT!
                return Redirect($"http://localhost:4200/auth/google/callback?token={token}");
            }
            catch (Exception ex)
            {
                return Redirect($"http://localhost:4200/auth/google/callback?error={ex.Message}");
            }
        }

        #endregion

        #region Standard Registration 

        /// <summary>
        /// Initiates sign-up by creating an OTP and emailing the user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            try
            {
                var result = await _authService.Register(dto);
                return Ok(new { message = result });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Validates OTP code, finalizes account creation, and returns success response.
        /// </summary>
        [HttpPost("verify-register-otp")]
        public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyOtpDTO dto)
        {
            try
            {
                var result = await _authService.VerifyRegistrationOtp(dto);
                return Ok(new { message = result });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        #endregion

        #region Password Management

        /// <summary>
        /// Issues a password reset trigger with OTP targeting the verified email address.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            try
            {
                var otp = await _otpService.GenerateAndSendOtpAsync(dto.Email);
                return Ok(new { message = "OTP sent to your registered email successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Verifies OTP code and securely resets the user's password.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithOtpDTO dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto);
                return Ok(new { message = "Password has been successfully changed" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        #endregion

        #region Identity & Session

        /// <summary>
        /// Validates user credentials returning a JWT active token session with a valid refresh token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                var result = await _authService.Login(dto);
                return Ok(result);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedException(ex.Message);
            }
        }

        /// <summary>
        /// Performs the GetProfile operation.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var user = await _authService.GetProfile(userId);
                return Ok(user);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new NotFoundException("Profile");
            }
        }

        /// <summary>
        /// Modifies the authenticated user profile information.
        /// </summary>
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDTO dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _authService.UpdateProfile(userId, dto);
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Performs the ChangePassword operation.
        /// </summary>
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _authService.ChangePassword(userId, dto);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Performs the Refresh operation.
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO dto)
        {
            try
            {
                var result = await _authService.Refresh(dto.RefreshToken);
                return Ok(result);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedException(ex.Message);
            }
        }

        #endregion
    }
}
