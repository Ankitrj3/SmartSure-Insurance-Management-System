using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories
{
    /// <summary>
    /// Represent or implements AuditLogRepository.
    /// </summary>
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AdminDbContext _context;

        public AuditLogRepository(AdminDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(AuditLog log)
        {
            await _context.AuditLogs.AddAsync(log);
        }

        /// <summary>
        /// Performs the GetPagedAsync operation.
        /// </summary>
        public async Task<List<AuditLog>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the GetTotalCountAsync operation.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }

        /// <summary>
        /// Performs the SaveChangesAsync operation.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
