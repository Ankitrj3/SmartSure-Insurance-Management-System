using MassTransit;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.AdminService.Consumers
{
    /// <summary>
    /// Represent or implements ClaimSubmittedConsumer.
    /// </summary>
    public class ClaimSubmittedConsumer : IConsumer<ClaimSubmittedEvent>
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<ClaimSubmittedConsumer> _logger;

        public ClaimSubmittedConsumer(IAuditService auditService, ILogger<ClaimSubmittedConsumer> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Performs the Consume operation.
        /// </summary>
        public async Task Consume(ConsumeContext<ClaimSubmittedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("[AdminService] ClaimSubmitted: ClaimId={ClaimId}, PolicyId={PolicyId}", msg.ClaimId, msg.PolicyId);

            await _auditService.LogAsync("ClaimSubmitted", "Claim", msg.ClaimId, msg.UserId,
                $"Claim submitted for policy {msg.PolicyId}. Description: {msg.Description}");
        }
    }
}
