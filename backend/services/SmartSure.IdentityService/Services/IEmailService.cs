namespace IdentityService.Services
{
    /// <summary>
    /// Represent or implements IEmailService.
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
