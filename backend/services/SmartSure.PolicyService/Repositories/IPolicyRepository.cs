using SmartSure.Shared.Contracts.DTOs;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    /// <summary>
    /// Represent or implements IPolicyRepository.
    /// </summary>
    public interface IPolicyRepository
    {
        Task<PagedResult<Policy>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PagedResult<Policy>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<Policy> GetByIdAsync(Guid policyId);
        Task AddAsync(Policy policy);
        Task UpdateAsync(Policy policy);
        Task CancelAsync(Guid policyId);
        Task DeleteAsync(Guid policyId);
        
        Task<PolicyDetail> GetDetailByPolicyIdAsync(Guid policyId);
        Task AddOrUpdateDetailAsync(PolicyDetail detail);
        
        Task<HomeDetail> GetHomeDetailByPolicyIdAsync(Guid policyId);
        Task AddOrUpdateHomeDetailAsync(HomeDetail detail);
        
        Task<VehicleDetail> GetVehicleDetailByPolicyIdAsync(Guid policyId);
        Task AddOrUpdateVehicleDetailAsync(VehicleDetail detail);
        
        Task AddPaymentAsync(Payment payment);

        Task SaveChangesAsync();
    }
}

