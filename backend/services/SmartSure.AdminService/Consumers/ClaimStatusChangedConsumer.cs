using MassTransit;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Contracts.Events;


namespace SmartSure.AdminService.Consumers
{
    /// <summary>
    /// Represent or implements ClaimStatusChangedConsumer.
    /// </summary>
    public class ClaimStatusChangedConsumer : IConsumer<ClaimStatusChangedEvent>
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<ClaimStatusChangedConsumer> _logger;

        public ClaimStatusChangedConsumer(IAuditService auditService, ILogger<ClaimStatusChangedConsumer> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Performs the Consume operation.
        /// </summary>
        public async Task Consume(ConsumeContext<ClaimStatusChangedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("[AdminService] ClaimStatusChanged: ClaimId={ClaimId}, {Old} -> {New}",
                msg.ClaimId, msg.OldStatus, msg.NewStatus);

            await _auditService.LogAsync("ClaimStatusChanged", "Claim", msg.ClaimId, null,
                $"Status changed from {msg.OldStatus} to {msg.NewStatus} by {msg.ChangedBy}");
        }
    }
}
