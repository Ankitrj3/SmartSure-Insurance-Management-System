using SmartSure.PolicyService.DTOs;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Represent or implements IPolicyMgmtService.
    /// </summary>
    public interface IPolicyMgmtService
    {
        Task<List<PolicyDTO>> GetUserPoliciesAsync(Guid userId);
        Task<List<PolicyDTO>> GetAllPoliciesAsync();
        Task<PolicyDTO> GetPolicyByIdAsync(Guid policyId);
        Task<PolicyDTO> CreatePolicyAsync(Guid userId, CreatePolicyDTO dto);
        Task CancelPolicyAsync(Guid policyId);
        
        Task<PolicyDetailDTO> GetPolicyDetailsAsync(Guid policyId);
        Task SavePolicyDetailsAsync(Guid policyId, SavePolicyDetailDTO dto);
        
        Task<decimal> GetPremiumAmountAsync(Guid policyId);
        
        Task<CreateHomeDetailDTO> GetHomeDetailAsync(Guid policyId);
        Task SaveHomeDetailAsync(Guid policyId, CreateHomeDetailDTO dto);
        
        Task<CreateVehicleDetailDTO> GetVehicleDetailAsync(Guid policyId);
        Task SaveVehicleDetailAsync(Guid policyId, CreateVehicleDetailDTO dto);

        /// <summary>
        /// Calculate IDV (vehicle) or Insurance Value (home) from asset details 
        /// without creating a policy. Used by the frontend to display the quote.
        /// </summary>
        Task<PolicyQuoteDTO> CalculateQuoteAsync(CreatePolicyDTO dto);

        /// <summary>
        /// Activate a pending policy after successful payment.
        /// </summary>
        Task ActivatePolicyAsync(Guid policyId);

        /// <summary>
        /// Terminate a policy (used for theft/total loss claims).
        /// </summary>
        Task TerminatePolicyAsync(Guid policyId);

        /// <summary>
        /// Increment approved claims count for a policy.
        /// </summary>
        Task IncrementApprovedClaimsCountAsync(Guid policyId);
    }
}
