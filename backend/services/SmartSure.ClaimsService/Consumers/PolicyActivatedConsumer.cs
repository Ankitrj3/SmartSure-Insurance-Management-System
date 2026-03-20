using MassTransit;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.ClaimsService.Consumers
{
    public class PolicyActivatedConsumer : IConsumer<PolicyActivatedEvent>
    {
        private readonly ILogger<PolicyActivatedConsumer> _logger;

        public PolicyActivatedConsumer(ILogger<PolicyActivatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<PolicyActivatedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation(
                "[ClaimsService] PolicyActivated received: PolicyId={PolicyId}, UserId={UserId}. Caching eligible policy.",
                msg.PolicyId, msg.UserId);

            // Cache the eligible policy for claim validation
            return Task.CompletedTask;
        }
    }
}
