namespace IdentityService.Services
{
    /// <summary>
    /// Represent or implements IGoogleAuthService.
    /// </summary>
    public interface IGoogleAuthService
    {
        string GetGoogleLoginUrl();
        Task<string> ProcessGoogleCallbackAsync(string code);
    }
}
