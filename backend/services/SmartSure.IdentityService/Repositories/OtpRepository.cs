using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories
{
    /// <summary>
    /// Represent or implements OtpRepository.
    /// </summary>
    public class OtpRepository : IOtpRepository
    {
        private readonly IdentityDbContext _context;

        public OtpRepository(IdentityDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetByEmailAsync operation.
        /// </summary>
        public async Task<OtpRecord> GetByEmailAsync(string email)
        {
            return await _context.Set<OtpRecord>().FirstOrDefaultAsync(o => o.Email == email);
        }

        /// <summary>
        /// Performs the GetAllByEmailAsync operation.
        /// </summary>
        public async Task<List<OtpRecord>> GetAllByEmailAsync(string email)
        {
            return await _context.Set<OtpRecord>().Where(o => o.Email == email).ToListAsync();
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(OtpRecord otpRecord)
        {
            await _context.Set<OtpRecord>().AddAsync(otpRecord);
        }

        /// <summary>
        /// Performs the RemoveAsync operation.
        /// </summary>
        public async Task RemoveAsync(OtpRecord otpRecord)
        {
            _context.Set<OtpRecord>().Remove(otpRecord);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Performs the RemoveRangeAsync operation.
        /// </summary>
        public async Task RemoveRangeAsync(List<OtpRecord> otpRecords)
        {
            _context.Set<OtpRecord>().RemoveRange(otpRecords);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Performs the SaveChangesAsync operation.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
