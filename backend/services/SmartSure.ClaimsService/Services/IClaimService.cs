using SmartSure.ClaimsService.DTOs;

namespace SmartSure.ClaimsService.Services
{
    public interface IClaimService
    {
        Task<ClaimResponseDTO> CreateClaimAsync(Guid userId, CreateClaimDTO dto);
        Task<ClaimResponseDTO?> GetClaimByIdAsync(Guid claimId);
        Task<List<ClaimResponseDTO>> GetUserClaimsAsync(Guid userId);
        Task<List<ClaimResponseDTO>> GetClaimsByPolicyAsync(Guid policyId);
        Task<ClaimResponseDTO> UpdateClaimAsync(Guid claimId, UpdateClaimDTO dto);
        Task SubmitClaimAsync(Guid claimId, Guid userId);
        Task<List<ClaimStatusHistoryDTO>> GetClaimHistoryAsync(Guid claimId);
        Task TransitionStatusAsync(Guid claimId, string newStatus, string changedBy, string? notes = null);
    }
}
