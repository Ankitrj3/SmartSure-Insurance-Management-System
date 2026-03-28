using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AdminDbContext _context;

        public AuditLogRepository(AdminDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditLog log)
        {
            await _context.AuditLogs.AddAsync(log);
        }

        public async Task<List<AuditLog>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
