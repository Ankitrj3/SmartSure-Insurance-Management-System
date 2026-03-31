namespace SmartSure.ClaimsService.Services
{
    /// <summary>
    /// Represent or implements IEmailService.
    /// </summary>
    public interface IEmailService
    {
        Task SendClaimApprovedEmailAsync(string toEmail, string userName, string claimId, decimal approvedAmount);
        Task SendClaimRejectedEmailAsync(string toEmail, string userName, string claimId, string reason);
        Task SendClaimUnderReviewEmailAsync(string toEmail, string userName, string claimId);
        Task SendClaimClosedEmailAsync(string toEmail, string userName, string claimId);
    }
}
