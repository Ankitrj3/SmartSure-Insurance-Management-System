using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Services
{
    public class AuditService : IAuditService
    {
        private readonly AdminDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AdminDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(string action, string entityType, Guid? entityId, Guid? actorId, string? details = null)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                ActorId = actorId,
                Details = details
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit: {Action} on {EntityType} [{EntityId}] by {ActorId}", action, entityType, entityId, actorId);
        }

        public async Task<List<AuditLogDTO>> GetAuditLogsAsync(int page, int pageSize)
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return logs.Select(l => new AuditLogDTO
            {
                Id = l.Id,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                ActorId = l.ActorId,
                Details = l.Details,
                Timestamp = l.Timestamp
            }).ToList();
        }

        public async Task<int> GetTotalAuditLogsCountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }
    }
}
