using MassTransit;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Contracts.Constants;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.ClaimsService.Consumers
{
    /// <summary>
    /// Represent or implements ClaimRejectedConsumer.
    /// </summary>
    public class ClaimRejectedConsumer : IConsumer<ClaimRejectedEvent>
    {
        private readonly IClaimService _claimService;
        private readonly ILogger<ClaimRejectedConsumer> _logger;

        public ClaimRejectedConsumer(IClaimService claimService, ILogger<ClaimRejectedConsumer> logger)
        {
            _claimService = claimService;
            _logger = logger;
        }

        /// <summary>
        /// Performs the Consume operation.
        /// </summary>
        public async Task Consume(ConsumeContext<ClaimRejectedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation(
                "[ClaimsService] ClaimRejected received: ClaimId={ClaimId}, AdminId={AdminId}, Reason={Reason}",
                msg.ClaimId, msg.AdminId, msg.Reason);

            await _claimService.TransitionStatusAsync(
                msg.ClaimId,
                ClaimStatus.Rejected,
                msg.AdminId.ToString(),
                msg.Reason);
        }
    }
}
