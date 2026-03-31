using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories
{
    /// <summary>
    /// Represent or implements IAuditLogRepository.
    /// </summary>
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<List<AuditLog>> GetPagedAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        Task SaveChangesAsync();
    }
}
