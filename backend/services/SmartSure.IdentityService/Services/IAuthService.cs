using IdentityService.DTOs;

namespace IdentityService.Services
{
    public interface IAuthService
    {
        Task<string> Register(RegisterDTO dto);
        Task<TokenResponseDTO> Login(LoginDTO dto);
        Task<UserDTO> GetProfile(string userId);
        Task UpdateProfile(string userId, UpdateUserDTO dto);
        Task ChangePassword(string userId, ChangePasswordDTO dto);
        Task<TokenResponseDTO> Refresh(string refreshToken);
    }
}
