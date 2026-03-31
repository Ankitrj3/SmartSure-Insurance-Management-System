namespace IdentityService.Services
{
    /// <summary>
    /// Represent or implements IOtpService.
    /// </summary>
    public interface IOtpService
    {
        Task<string> GenerateAndSendOtpAsync(string email);
        Task<bool> ValidateOtpAsync(string email, string otp);
    }
}
