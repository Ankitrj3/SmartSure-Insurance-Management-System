using MassTransit;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.AdminService.Consumers
{
    /// <summary>
    /// Represent or implements PolicyCancelledConsumer.
    /// </summary>
    public class PolicyCancelledConsumer : IConsumer<PolicyCancelledEvent>
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<PolicyCancelledConsumer> _logger;

        public PolicyCancelledConsumer(IAuditService auditService, ILogger<PolicyCancelledConsumer> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Performs the Consume operation.
        /// </summary>
        public async Task Consume(ConsumeContext<PolicyCancelledEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("[AdminService] PolicyCancelled: PolicyId={PolicyId}, Reason={Reason}", msg.PolicyId, msg.Reason);

            await _auditService.LogAsync("PolicyCancelled", "Policy", msg.PolicyId, msg.UserId,
                $"Policy cancelled. Reason: {msg.Reason}");
        }
    }
}
