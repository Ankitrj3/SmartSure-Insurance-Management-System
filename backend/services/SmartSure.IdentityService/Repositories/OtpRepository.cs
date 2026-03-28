using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly IdentityDbContext _context;

        public OtpRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<OtpRecord> GetByEmailAsync(string email)
        {
            return await _context.Set<OtpRecord>().FirstOrDefaultAsync(o => o.Email == email);
        }

        public async Task<List<OtpRecord>> GetAllByEmailAsync(string email)
        {
            return await _context.Set<OtpRecord>().Where(o => o.Email == email).ToListAsync();
        }

        public async Task AddAsync(OtpRecord otpRecord)
        {
            await _context.Set<OtpRecord>().AddAsync(otpRecord);
        }

        public async Task RemoveAsync(OtpRecord otpRecord)
        {
            _context.Set<OtpRecord>().Remove(otpRecord);
            await Task.CompletedTask;
        }

        public async Task RemoveRangeAsync(List<OtpRecord> otpRecords)
        {
            _context.Set<OtpRecord>().RemoveRange(otpRecords);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
