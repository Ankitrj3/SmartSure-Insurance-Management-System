using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    /// <summary>
    /// Represent or implements IPaymentRepository.
    /// </summary>
    public interface IPaymentRepository
    {
        Task<List<Payment>> GetByPolicyIdAsync(Guid policyId);
        Task<List<Payment>> GetByUserIdAsync(Guid userId);
        Task<Payment> GetByIdAsync(Guid paymentId);
        Task AddAsync(Payment payment);
        Task SaveChangesAsync();
    }
}
