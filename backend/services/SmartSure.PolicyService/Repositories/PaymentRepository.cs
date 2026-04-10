using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    /// <summary>
    /// Represent or implements PaymentRepository.
    /// </summary>
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PolicyDbContext _context;

        public PaymentRepository(PolicyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetByPolicyIdAsync operation.
        /// </summary>
        public async Task<List<Payment>> GetByPolicyIdAsync(Guid policyId)
        {
            return await _context.Payments
                .Where(p => p.PolicyId == policyId)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the GetByUserIdAsync operation.
        /// </summary>
        public async Task<List<Payment>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Payments
                .Include(p => p.Policy) // Include policy to navigate to User
                .Where(p => p.Policy.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the GetByIdAsync operation.
        /// </summary>
        public async Task<Payment> GetByIdAsync(Guid paymentId)
        {
            return await _context.Payments.FindAsync(paymentId);
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
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
