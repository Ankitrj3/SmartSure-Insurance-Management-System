using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    public interface IPolicyRepository
    {
        Task<List<Policy>> GetByUserIdAsync(Guid userId);
        Task<List<Policy>> GetAllAsync();
        Task<Policy> GetByIdAsync(Guid policyId);
        Task AddAsync(Policy policy);
        Task UpdateAsync(Policy policy);
        Task CancelAsync(Guid policyId);
        
        Task<PolicyDetail> GetDetailByPolicyIdAsync(Guid policyId);
        Task AddOrUpdateDetailAsync(PolicyDetail detail);
        
        Task<HomeDetail> GetHomeDetailByPolicyIdAsync(Guid policyId);
        Task AddOrUpdateHomeDetailAsync(HomeDetail detail);
        
        Task<VehicleDetail> GetVehicleDetailByPolicyIdAsync(Guid policyId);
        Task AddOrUpdateVehicleDetailAsync(VehicleDetail detail);
        
        Task SaveChangesAsync();
    }
}

