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

            bool shouldNotify = msg.NewStatus == ClaimStatus.Approved
                             || msg.NewStatus == ClaimStatus.Rejected
                             || msg.NewStatus == ClaimStatus.UnderReview;

            if (shouldNotify)
            {
                var user = await _userRepository.GetByIdAsync(msg.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    string subject;
                    string body;

                    if (msg.NewStatus == ClaimStatus.UnderReview)
                    {
                        subject = $"Your Claim {msg.ClaimId} is Under Review";
                        body = $"Hello {user.FullName},<br/><br/>"
                             + $"Your insurance claim with ID <b>{msg.ClaimId}</b> is now <b>Under Review</b>.<br/><br/>"
                             + "Our team is currently reviewing your claim. We will notify you once a decision has been made.";
                    }
                    else if (msg.NewStatus == ClaimStatus.Approved)
                    {
                        subject = $"Your Claim {msg.ClaimId} has been Approved";
                        body = $"Hello {user.FullName},<br/><br/>"
                             + $"Great news! Your insurance claim with ID <b>{msg.ClaimId}</b> has been <b>Approved</b>.";
                    }
                    else // Rejected
                    {
                        subject = $"Your Claim {msg.ClaimId} has been Rejected";
                        body = $"Hello {user.FullName},<br/><br/>"
                             + $"We regret to inform you that your insurance claim with ID <b>{msg.ClaimId}</b> has been <b>Rejected</b>.";
                        if (!string.IsNullOrEmpty(msg.Reason))
                        {
                            body += $"<br/><br/><b>Reason for Rejection:</b> {msg.Reason}";
                        }
                    }

                    body += "<br/><br/>If you have any questions, please contact our support team.";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
            }
        }
    }
}
