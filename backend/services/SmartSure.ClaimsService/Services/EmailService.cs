using System.Net;
using System.Net.Mail;

namespace SmartSure.ClaimsService.Services
{
    /// <summary>
    /// Represent or implements EmailService.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Performs the SendClaimApprovedEmailAsync operation.
        /// </summary>
        public async Task SendClaimApprovedEmailAsync(string toEmail, string userName, string claimId, decimal approvedAmount)
        {
            var subject = "✅ Your Claim Has Been Approved - SmartSure Insurance";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #4caf50 0%, #2e7d32 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #e0e0e0; }}
        .amount {{ font-size: 32px; font-weight: bold; color: #4caf50; margin: 20px 0; }}
        .claim-id {{ background: #e8f5e9; padding: 10px; border-left: 4px solid #4caf50; margin: 20px 0; }}
        .footer {{ background: #f5f5f5; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #4caf50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Claim Approved!</h1>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>
            
            <p>We are pleased to inform you that your insurance claim has been <strong>approved</strong>.</p>
            
            <div class='claim-id'>
                <strong>Claim ID:</strong> {claimId}
            </div>
            
            <p><strong>Approved Amount:</strong></p>
            <div class='amount'>₹{approvedAmount:N2}</div>
            
            <p>The approved amount will be processed and transferred to your registered bank account within 5-7 business days.</p>
            
            <p><strong>Next Steps:</strong></p>
            <ul>
                <li>You will receive a payment confirmation email once the transfer is initiated</li>
                <li>Please ensure your bank account details are up to date</li>
                <li>For any queries, contact our support team</li>
            </ul>
            
            <p>Thank you for choosing SmartSure Insurance. We're here to protect what matters most to you.</p>
            
            <p>Best regards,<br>
            <strong>SmartSure Claims Team</strong></p>
        </div>
        <div class='footer'>
            <p>SmartSure Insurance Management System</p>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        /// <summary>
        /// Performs the SendClaimRejectedEmailAsync operation.
        /// </summary>
        public async Task SendClaimRejectedEmailAsync(string toEmail, string userName, string claimId, string reason)
        {
            var subject = "❌ Claim Decision - SmartSure Insurance";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #f44336 0%, #c62828 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #e0e0e0; }}
        .claim-id {{ background: #ffebee; padding: 10px; border-left: 4px solid #f44336; margin: 20px 0; }}
        .reason-box {{ background: #fff3e0; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #ff9800; }}
        .footer {{ background: #f5f5f5; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 8px 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Claim Decision Notice</h1>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>
            
            <p>After careful review of your insurance claim, we regret to inform you that your claim has been <strong>declined</strong>.</p>
            
            <div class='claim-id'>
                <strong>Claim ID:</strong> {claimId}
            </div>
            
            <div class='reason-box'>
                <strong>Reason for Rejection:</strong><br>
                {reason}
            </div>
            
            <p><strong>What You Can Do:</strong></p>
            <ul>
                <li>Review your policy terms and conditions</li>
                <li>Contact our support team for clarification</li>
                <li>Submit an appeal if you believe this decision was made in error</li>
                <li>Provide additional documentation if available</li>
            </ul>
            
            <p>We understand this may be disappointing. Our team is available to discuss this decision and answer any questions you may have.</p>
            
            <p><strong>Contact Support:</strong> support@smartsure.com | 1800-XXX-XXXX</p>
            
            <p>Best regards,<br>
            <strong>SmartSure Claims Team</strong></p>
        </div>
        <div class='footer'>
            <p>SmartSure Insurance Management System</p>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        /// <summary>
        /// Performs the SendClaimUnderReviewEmailAsync operation.
        /// </summary>
        public async Task SendClaimUnderReviewEmailAsync(string toEmail, string userName, string claimId)
        {
            var subject = "🔍 Your Claim is Under Review - SmartSure Insurance";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #e0e0e0; }}
        .claim-id {{ background: #fff3e0; padding: 10px; border-left: 4px solid #ff9800; margin: 20px 0; }}
        .info-box {{ background: #e3f2fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ background: #f5f5f5; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 8px 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⏳ Claim Under Review</h1>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>
            
            <p>Your insurance claim is currently <strong>under review</strong> by our claims assessment team.</p>
            
            <div class='claim-id'>
                <strong>Claim ID:</strong> {claimId}
            </div>
            
            <div class='info-box'>
                <strong>What This Means:</strong><br>
                Our team is conducting a detailed investigation of your claim. This may include verifying documentation, assessing damage reports, and ensuring all policy conditions are met.
            </div>
            
            <p><strong>Expected Timeline:</strong></p>
            <ul>
                <li>Standard review: 3-5 business days</li>
                <li>Complex cases: 7-10 business days</li>
                <li>You will be notified immediately once a decision is made</li>
            </ul>
            
            <p><strong>During This Time:</strong></p>
            <ul>
                <li>Please keep your contact information updated</li>
                <li>Be ready to provide additional documentation if requested</li>
                <li>Check your email regularly for updates</li>
            </ul>
            
            <p>We appreciate your patience as we carefully review your claim.</p>
            
            <p>Best regards,<br>
            <strong>SmartSure Claims Team</strong></p>
        </div>
        <div class='footer'>
            <p>SmartSure Insurance Management System</p>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        /// <summary>
        /// Performs the SendClaimClosedEmailAsync operation.
        /// </summary>
        public async Task SendClaimClosedEmailAsync(string toEmail, string userName, string claimId)
        {
            var subject = "🔒 Your Claim Has Been Closed - SmartSure Insurance";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #9e9e9e 0%, #616161 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #e0e0e0; }}
        .claim-id {{ background: #f5f5f5; padding: 10px; border-left: 4px solid #9e9e9e; margin: 20px 0; }}
        .footer {{ background: #f5f5f5; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 8px 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Claim Closed</h1>
        </div>
        <div class='content'>
            <p>Dear {userName},</p>
            
            <p>Your insurance claim has been <strong>closed</strong> and archived in our system.</p>
            
            <div class='claim-id'>
                <strong>Claim ID:</strong> {claimId}
            </div>
            
            <p><strong>What This Means:</strong></p>
            <ul>
                <li>This claim has been finalized and no further action is required</li>
                <li>All processing related to this claim is complete</li>
                <li>The claim record is archived for your reference</li>
            </ul>
            
            <p>If you have any questions about this closure or need to file a new claim, please contact our support team.</p>
            
            <p><strong>Need Help?</strong><br>
            Contact: support@smartsure.com | 1800-XXX-XXXX</p>
            
            <p>Thank you for choosing SmartSure Insurance.</p>
            
            <p>Best regards,<br>
            <strong>SmartSure Claims Team</strong></p>
        </div>
        <div class='footer'>
            <p>SmartSure Insurance Management System</p>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];
            var fromEmail = _config["Email:FromEmail"] ?? "noreply@smartsure.com";

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                // In local dev without real keys, just log it
                _logger.LogInformation("[EMAIL SIMULATION] To: {ToEmail}, Subject: {Subject}", toEmail, subject);
                Console.WriteLine($"[EMAIL] To: {toEmail}\nSubject: {subject}\n");
                return;
            }

            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                    Timeout = 10000
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            }
        }
    }
}
