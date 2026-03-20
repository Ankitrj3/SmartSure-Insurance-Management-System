using MassTransit;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.AdminService.Consumers
{
    public class PolicyActivatedConsumer : IConsumer<PolicyActivatedEvent>
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<PolicyActivatedConsumer> _logger;

        public PolicyActivatedConsumer(IAuditService auditService, ILogger<PolicyActivatedConsumer> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PolicyActivatedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("[AdminService] PolicyActivated: PolicyId={PolicyId}, UserId={UserId}", msg.PolicyId, msg.UserId);

            await _auditService.LogAsync("PolicyActivated", "Policy", msg.PolicyId, msg.UserId,
                $"Policy activated. Type: {msg.TypeId}, SubType: {msg.SubTypeId}");
        }
    }
}
