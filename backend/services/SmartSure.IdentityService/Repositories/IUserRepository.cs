using IdentityService.Models;

namespace IdentityService.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(Guid id);
        Task<List<User>> GetAllAsync();
        Task AddAsync(User user);
        Task SaveChangesAsync();
        void Delete(User user);
        Task<Role> GetRoleByIdAsync(Guid roleId);
        Task AddUserRoleAsync(UserRole userRole);
    }
}
