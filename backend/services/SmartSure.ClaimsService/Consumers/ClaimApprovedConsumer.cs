using MassTransit;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Contracts.Constants;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.ClaimsService.Consumers
{
    /// <summary>
    /// Represent or implements ClaimApprovedConsumer.
    /// </summary>
    public class ClaimApprovedConsumer : IConsumer<ClaimApprovedEvent>
    {
        private readonly IClaimService _claimService;
        private readonly ILogger<ClaimApprovedConsumer> _logger;

        public ClaimApprovedConsumer(IClaimService claimService, ILogger<ClaimApprovedConsumer> logger)
        {
            _claimService = claimService;
            _logger = logger;
        }

        /// <summary>
        /// Performs the Consume operation.
        /// </summary>
        public async Task Consume(ConsumeContext<ClaimApprovedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation(
                "[ClaimsService] ClaimApproved received: ClaimId={ClaimId}, AdminId={AdminId}",
                msg.ClaimId, msg.AdminId);

            await _claimService.TransitionStatusAsync(
                msg.ClaimId,
                ClaimStatus.Approved,
                msg.AdminId.ToString(),
                msg.Notes);
        }
    }
}
