using IdentityService.Models;

namespace IdentityService.Repositories
{
    /// <summary>
    /// Represent or implements IOtpRepository.
    /// </summary>
    public interface IOtpRepository
    {
        Task<OtpRecord> GetByEmailAsync(string email);
        Task<List<OtpRecord>> GetAllByEmailAsync(string email);
        Task AddAsync(OtpRecord otpRecord);
        Task RemoveAsync(OtpRecord otpRecord);
        Task RemoveRangeAsync(List<OtpRecord> otpRecords);
        Task SaveChangesAsync();
    }
}
