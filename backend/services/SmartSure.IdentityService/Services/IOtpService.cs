namespace IdentityService.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAndSendOtpAsync(string email);
        Task<bool> ValidateOtpAsync(string email, string otp);
    }
}
