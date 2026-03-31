using IdentityService.DTOs;

namespace IdentityService.Services
{
    /// <summary>
    /// Represent or implements IAuthService.
    /// </summary>
    public interface IAuthService
    {
        Task<string> Register(RegisterDTO dto);
        Task<string> VerifyRegistrationOtp(VerifyOtpDTO dto);
        Task<TokenResponseDTO> Login(LoginDTO dto);
        Task<UserDTO> GetProfile(string userId);
        Task<List<UserDTO>> GetAllUsers();
        Task UpdateProfile(string userId, UpdateUserDTO dto);
        Task ChangePassword(string userId, ChangePasswordDTO dto);
        Task<TokenResponseDTO> Refresh(string refreshToken);
        Task ResetPasswordAsync(ResetPasswordWithOtpDTO dto);
    }
}
