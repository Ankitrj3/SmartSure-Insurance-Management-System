using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services
{
    public class OtpService : IOtpService
    {
        private readonly IdentityDbContext _context;
        private readonly IEmailService _emailService;

        public OtpService(IdentityDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<string> GenerateAndSendOtpAsync(string email)
        {
            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new Exception("User not found");

            var otpRecord = new OtpRecord
            {
                UserId = user.UserId,
                Email = email,
                Otp = otp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10),
                Attempts = 0
            };

            var existingOtps = _context.Set<OtpRecord>().Where(o => o.Email == email).ToList();
            if (existingOtps.Any())
            {
                _context.Set<OtpRecord>().RemoveRange(existingOtps);
            }

            _context.Set<OtpRecord>().Add(otpRecord);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(email, "Your Password Reset OTP", $"Your 6-digit OTP is: <b>{otp}</b>. It expires in 10 minutes.");
            return otp;
        }

        public async Task<bool> ValidateOtpAsync(string email, string otp)
        {
            var record = await _context.Set<OtpRecord>().FirstOrDefaultAsync(o => o.Email == email);
            if (record == null || record.ExpirationTime < DateTime.UtcNow) return false;

            if (record.Attempts >= 3)
            {
                _context.Set<OtpRecord>().Remove(record);
                await _context.SaveChangesAsync();
                return false;
            }

            if (record.Otp == otp)
            {
                _context.Set<OtpRecord>().Remove(record);
                await _context.SaveChangesAsync();
                return true;
            }

            record.Attempts++;
            await _context.SaveChangesAsync();
            return false;
        }
    }
}
