using System.Net;
using System.Net.Mail;

namespace IdentityService.Services
{
    /// <summary>
    /// Represent or implements EmailService.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) { _config = config; }

        /// <summary>
        /// Performs the SendEmailAsync operation.
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];
            var fromEmail = _config["Email:FromEmail"];

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                // In local dev without real keys, just log it
                Console.WriteLine($"[EMAIL SIMULATION] To: {toEmail}, Subject: {subject}, Body: {body}");
                return;
            }

            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
                Timeout = 10000 // Set 10s timeout so it doesn't hang indefinitely 
            };
            
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
