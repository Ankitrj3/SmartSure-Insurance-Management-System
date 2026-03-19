using IdentityService.DTOs;

namespace IdentityService.Services
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetUsers();
        Task AssignRole(Guid userId, Guid roleId);
        Task DeleteUser(Guid userId);
    }
}
