using MassTransit;
using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using SmartSure.Shared.Contracts.Constants;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.ClaimsService.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ClaimsDbContext _context;
        private readonly IBus _bus;
        private readonly ILogger<ClaimService> _logger;

        public ClaimService(ClaimsDbContext context, IBus bus, ILogger<ClaimService> logger)
        {
            _context = context;
            _bus = bus;
            _logger = logger;
        }

        public async Task<ClaimResponseDTO> CreateClaimAsync(Guid userId, CreateClaimDTO dto)
        {
            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                PolicyId = dto.PolicyId,
                UserId = userId,
                Description = dto.Description,
                ClaimAmount = dto.ClaimAmount,
                Status = ClaimStatus.Draft
            };

            // Add initial status history
            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                ClaimId = claim.ClaimId,
                OldStatus = "",
                NewStatus = ClaimStatus.Draft,
                ChangedBy = userId.ToString(),
                Notes = "Claim created"
            });

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Claim {ClaimId} created for policy {PolicyId} by user {UserId}", claim.ClaimId, dto.PolicyId, userId);

            return MapToDto(claim);
        }

        public async Task<ClaimResponseDTO?> GetClaimByIdAsync(Guid claimId)
        {
            var claim = await _context.Claims
                .Include(c => c.Documents)
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            return claim == null ? null : MapToDto(claim);
        }

        public async Task<List<ClaimResponseDTO>> GetUserClaimsAsync(Guid userId)
        {
            var claims = await _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return claims.Select(MapToDto).ToList();
        }

        public async Task<List<ClaimResponseDTO>> GetClaimsByPolicyAsync(Guid policyId)
        {
            var claims = await _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.PolicyId == policyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return claims.Select(MapToDto).ToList();
        }

        public async Task<ClaimResponseDTO> UpdateClaimAsync(Guid claimId, UpdateClaimDTO dto)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) throw new KeyNotFoundException("Claim not found.");

            if (claim.Status != ClaimStatus.Draft)
                throw new InvalidOperationException("Only draft claims can be updated.");

            if (!string.IsNullOrEmpty(dto.Description))
                claim.Description = dto.Description;

            if (dto.ClaimAmount.HasValue)
                claim.ClaimAmount = dto.ClaimAmount.Value;

            claim.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Claim {ClaimId} updated", claimId);

            return MapToDto(claim);
        }

        public async Task SubmitClaimAsync(Guid claimId, Guid userId)
        {
            var claim = await _context.Claims
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null) throw new KeyNotFoundException("Claim not found.");
            if (claim.Status != ClaimStatus.Draft)
                throw new InvalidOperationException("Only draft claims can be submitted.");

            var oldStatus = claim.Status;
            claim.Status = ClaimStatus.Submitted;
            claim.UpdatedAt = DateTime.UtcNow;

            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                ClaimId = claimId,
                OldStatus = oldStatus,
                NewStatus = ClaimStatus.Submitted,
                ChangedBy = userId.ToString(),
                Notes = "Claim submitted for review"
            });

            await _context.SaveChangesAsync();

            // Publish event
            await _bus.Publish(new ClaimSubmittedEvent(
                claimId, claim.PolicyId, claim.UserId, claim.Description, DateTime.UtcNow));

            await _bus.Publish(new ClaimStatusChangedEvent(
                claimId, oldStatus, ClaimStatus.Submitted, userId.ToString(), DateTime.UtcNow, claim.UserId));

            _logger.LogInformation("Claim {ClaimId} submitted", claimId);
        }

        public async Task<List<ClaimStatusHistoryDTO>> GetClaimHistoryAsync(Guid claimId)
        {
            var history = await _context.ClaimStatusHistory
                .Where(h => h.ClaimId == claimId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            return history.Select(h => new ClaimStatusHistoryDTO
            {
                Id = h.Id,
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                Notes = h.Notes,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt
            }).ToList();
        }

        public async Task TransitionStatusAsync(Guid claimId, string newStatus, string changedBy, string? notes = null)
        {
            var claim = await _context.Claims
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null) throw new KeyNotFoundException("Claim not found.");

            var oldStatus = claim.Status;
            claim.Status = newStatus;
            claim.UpdatedAt = DateTime.UtcNow;

            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                ClaimId = claimId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = changedBy,
                Notes = notes
            });

            await _context.SaveChangesAsync();

            await _bus.Publish(new ClaimStatusChangedEvent(
                claimId, oldStatus, newStatus, changedBy, DateTime.UtcNow, claim.UserId));

            _logger.LogInformation("Claim {ClaimId} status changed from {Old} to {New}", claimId, oldStatus, newStatus);
        }

        private static ClaimResponseDTO MapToDto(Claim claim)
        {
            return new ClaimResponseDTO
            {
                ClaimId = claim.ClaimId,
                PolicyId = claim.PolicyId,
                UserId = claim.UserId,
                Description = claim.Description,
                Status = claim.Status,
                ClaimAmount = claim.ClaimAmount,
                ApprovedAmount = claim.ApprovedAmount,
                RejectionReason = claim.RejectionReason,
                CreatedAt = claim.CreatedAt,
                UpdatedAt = claim.UpdatedAt,
                Documents = claim.Documents?.Select(d => new DocumentResponseDTO
                {
                    DocumentId = d.DocumentId,
                    ClaimId = d.ClaimId,
                    FileName = d.FileName,
                    FileUrl = d.FileUrl,
                    ContentType = d.ContentType,
                    FileSize = d.FileSize,
                    UploadedAt = d.UploadedAt
                }).ToList() ?? new()
            };
        }
    }
}
