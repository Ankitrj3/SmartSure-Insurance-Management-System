using SmartSure.ClaimsService.DTOs;
using SmartSure.Shared.Contracts.DTOs;

namespace SmartSure.ClaimsService.Services
{
    /// <summary>
    /// Represent or implements IClaimService.
    /// </summary>
    public interface IClaimService
    {
        Task<ClaimResponseDTO> CreateClaimAsync(Guid userId, CreateClaimDTO dto);
        Task<ClaimResponseDTO?> GetClaimByIdAsync(Guid claimId);
        Task<PagedResult<ClaimResponseDTO>> GetUserClaimsAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PagedResult<ClaimResponseDTO>> GetAllClaimsAsync(int page = 1, int pageSize = 10);
        Task<List<ClaimResponseDTO>> GetClaimsByPolicyAsync(Guid policyId);
        Task<ClaimResponseDTO> UpdateClaimAsync(Guid claimId, UpdateClaimDTO dto);
        Task SubmitClaimAsync(Guid claimId, Guid userId);
        Task<List<ClaimStatusHistoryDTO>> GetClaimHistoryAsync(Guid claimId);
        Task TransitionStatusAsync(Guid claimId, string newStatus, string changedBy, string? notes = null);
        Task ApproveClaimAsync(Guid claimId, decimal approvedAmount, string? notes, string adminId);
        Task RejectClaimAsync(Guid claimId, string reason, string adminId);
        Task UpdateClaimStatusAsync(Guid claimId, string newStatus, string? notes, string changedBy);
    }
}
