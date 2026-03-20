namespace IdentityService.Services
{
    public interface IGoogleAuthService
    {
        string GetGoogleLoginUrl();
        Task<string> ProcessGoogleCallbackAsync(string code);
    }
}
