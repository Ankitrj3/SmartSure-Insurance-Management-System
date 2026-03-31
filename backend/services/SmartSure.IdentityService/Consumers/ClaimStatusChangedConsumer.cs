using MassTransit;
using IdentityService.Services;
using SmartSure.Shared.Contracts.Events;
using IdentityService.Repositories;
using SmartSure.Shared.Contracts.Constants;

namespace IdentityService.Consumers
{
    /// <summary>
    /// Represent or implements ClaimStatusChangedConsumer.
    /// </summary>
    public class ClaimStatusChangedConsumer : IConsumer<ClaimStatusChangedEvent>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public ClaimStatusChangedConsumer(IUserRepository userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        /// <summary>
        /// Performs the Consume operation.
        /// </summary>
        public async Task Consume(ConsumeContext<ClaimStatusChangedEvent> context)
        {
            var msg = context.Message;
            if (msg.NewStatus == ClaimStatus.Approved || msg.NewStatus == ClaimStatus.Rejected)
            {
                var user = await _userRepository.GetByIdAsync(msg.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    string subject = $"Your Claim {msg.ClaimId} was {msg.NewStatus.ToUpper()}";
                    string body = $"Hello {user.FullName},<br/><br/>Your insurance claim with ID <b>{msg.ClaimId}</b> has been <b>{msg.NewStatus}</b>.";
                    if (msg.NewStatus == ClaimStatus.Rejected && !string.IsNullOrEmpty(msg.Reason))
                    {
                        body += $"<br/><br/><b>Reason for Rejection:</b> {msg.Reason}";
                    }
                    body += "<br/><br/>If you have any questions, please contact our support team.";
                    
                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
            }
        }
    }
}
