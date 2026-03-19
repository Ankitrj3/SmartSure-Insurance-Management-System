using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PolicyDbContext _context;

        public PaymentRepository(PolicyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payment>> GetByPolicyIdAsync(Guid policyId)
        {
            return await _context.Payments
                .Where(p => p.PolicyId == policyId)
                .ToListAsync();
        }

        public async Task<Payment> GetByIdAsync(Guid paymentId)
        {
            return await _context.Payments.FindAsync(paymentId);
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
