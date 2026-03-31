using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Models;
using SmartSure.AdminService.Repositories;

namespace SmartSure.AdminService.Services
{
    /// <summary>
    /// Represent or implements AuditService.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _repository;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IAuditLogRepository repository, ILogger<AuditService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Performs the LogAsync operation.
        /// </summary>
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

            await _repository.AddAsync(log);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Audit: {Action} on {EntityType} [{EntityId}] by {ActorId}", action, entityType, entityId, actorId);
        }

        /// <summary>
        /// Performs the GetAuditLogsAsync operation.
        /// </summary>
        public async Task<List<AuditLogDTO>> GetAuditLogsAsync(int page, int pageSize)
        {
            var logs = await _repository.GetPagedAsync(page, pageSize);

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

        /// <summary>
        /// Performs the GetTotalAuditLogsCountAsync operation.
        /// </summary>
        public async Task<int> GetTotalAuditLogsCountAsync()
        {
            return await _repository.GetTotalCountAsync();
        }
    }
}
