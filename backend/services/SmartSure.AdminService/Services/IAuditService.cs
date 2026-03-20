using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityType, Guid? entityId, Guid? actorId, string? details = null);
        Task<List<AuditLogDTO>> GetAuditLogsAsync(int page, int pageSize);
        Task<int> GetTotalAuditLogsCountAsync();
    }
}
