using IdentityService.Models;
using IdentityService.Repositories;

namespace IdentityService.Services
{
    /// <summary>
    /// Represent or implements OtpService.
    /// </summary>
    public class OtpService : IOtpService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;

        public OtpService(IUserRepository userRepository, IOtpRepository otpRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _emailService = emailService;
        }

        /// <summary>
        /// Performs the GenerateAndSendOtpAsync operation.
        /// </summary>
        public async Task<string> GenerateAndSendOtpAsync(string email)
        {
            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new Exception("User not found");

            var otpRecord = new OtpRecord
            {
                UserId = user.UserId,
                Email = email,
                Otp = otp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10),
                Attempts = 0
            };

            var existingOtps = await _otpRepository.GetAllByEmailAsync(email);
            if (existingOtps.Any())
            {
                await _otpRepository.RemoveRangeAsync(existingOtps);
            }

            await _otpRepository.AddAsync(otpRecord);
            await _otpRepository.SaveChangesAsync();

            await _emailService.SendEmailAsync(email, "Your Password Reset OTP", $"Your 6-digit OTP is: <b>{otp}</b>. It expires in 10 minutes.");
            return otp;
        }

        /// <summary>
        /// Performs the ValidateOtpAsync operation.
        /// </summary>
        public async Task<bool> ValidateOtpAsync(string email, string otp)
        {
            var record = await _otpRepository.GetByEmailAsync(email);
            if (record == null || record.ExpirationTime < DateTime.UtcNow) return false;

            if (record.Attempts >= 3)
            {
                await _otpRepository.RemoveAsync(record);
                await _otpRepository.SaveChangesAsync();
                return false;
            }

            if (record.Otp == otp)
            {
                await _otpRepository.RemoveAsync(record);
                await _otpRepository.SaveChangesAsync();
                return true;
            }

            record.Attempts++;
            await _otpRepository.SaveChangesAsync();
            return false;
        }
    }
}
