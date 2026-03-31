using IdentityService.DTOs;

namespace IdentityService.Services
{
    /// <summary>
    /// Represent or implements IUserService.
    /// </summary>
    public interface IUserService
    {
        Task<List<UserDTO>> GetUsers();
        Task AssignRole(Guid userId, Guid roleId);
        Task DeleteUser(Guid userId);
    }
}
