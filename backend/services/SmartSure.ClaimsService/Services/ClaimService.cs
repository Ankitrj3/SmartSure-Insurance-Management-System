using MassTransit;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using SmartSure.ClaimsService.Repositories;
using SmartSure.Shared.Contracts.DTOs;
using SmartSure.Shared.Contracts.Constants;
using SmartSure.Shared.Contracts.Events;
using SmartSure.Shared.Contracts.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace SmartSure.ClaimsService.Services
{
    /// <summary>
    /// Core service responsible for processing insurance claims, transitioning states,
    /// and managing business rules (e.g., maximum claims per policy, total loss handling).
    /// </summary>
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimStatusHistoryRepository _historyRepository;
        private readonly IBus _bus;
        private readonly ILogger<ClaimService> _logger;
        private readonly IEmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimService(
            IClaimRepository claimRepository,
            IClaimStatusHistoryRepository historyRepository,
            IBus bus, 
            ILogger<ClaimService> logger,
            IEmailService emailService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _claimRepository = claimRepository;
            _historyRepository = historyRepository;
            _bus = bus;
            _logger = logger;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<(string Email, string FullName)> GetUserDetailsAsync(Guid userId, string? token = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Add("Authorization", token.StartsWith("Bearer ") ? token : $"Bearer {token}");
                }
                else
                {
                    // Attempt to get from current context if not provided
                    var currentToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        client.DefaultRequestHeaders.Add("Authorization", currentToken);
                    }
                }

                var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";
                var response = await client.GetAsync($"{gatewayUrl}/auth/users/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = System.Text.Json.JsonDocument.Parse(content);
                    var email = user.RootElement.GetProperty("email").GetString() ?? "";
                    var fullName = user.RootElement.GetProperty("fullName").GetString() ?? "Valued Customer";
                    return (email, fullName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user details for {UserId}", userId);
            }
            
            return ("", "Valued Customer");
        }

        /// <summary>
        /// Creates a new draft claim. Enforces business constraints such as no pending claims allow,
        /// max claim limits, and previous total loss claims.
        /// </summary>
        /// <param name="userId">The ID of the user creating the claim.</param>
        /// <param name="dto">The claim creation payload.</param>
        /// <returns>The created claim mapped to a comprehensive DTO.</returns>
        public async Task<ClaimResponseDTO> CreateClaimAsync(Guid userId, CreateClaimDTO dto)
        {
            var existingClaims = await _claimRepository.GetByPolicyIdAsync(dto.PolicyId);
            
            // Check for pending claims
            var pendingClaimExists = existingClaims.Any(c =>
                c.Status == ClaimStatus.Draft || 
                c.Status == ClaimStatus.Submitted || 
                c.Status == ClaimStatus.UnderReview);

            if (pendingClaimExists)
                throw new ConflictException("You already filed a claim for this insurance. It is currently under review.");

            // Check approved claims count (max 3 per policy)
            var approvedClaimsCount = existingClaims.Count(c => c.Status == ClaimStatus.Approved);
            if (approvedClaimsCount >= 3)
                throw new ConflictException("Maximum claim limit reached. You can only file 3 claims per insurance policy.");

            // Check if policy was terminated due to theft/total loss
            var hasTheftClaim = existingClaims.Any(c => 
                c.ClaimType == "Stolen" && c.Status == ClaimStatus.Approved);
            if (hasTheftClaim)
                throw new ConflictException("This policy has been terminated due to a previous theft claim. No further claims can be filed.");

            var claim = new Claim
            {
                ClaimId              = Guid.NewGuid(),
                PolicyId             = dto.PolicyId,
                UserId               = userId,
                Description          = dto.Description,
                ClaimAmount          = dto.ClaimAmount,
                ClaimType            = dto.ClaimType,
                IsCompletelyDamaged  = dto.IsCompletelyDamaged,
                Status               = ClaimStatus.Draft
            };

            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                ClaimId    = claim.ClaimId,
                OldStatus  = "",
                NewStatus  = ClaimStatus.Draft,
                ChangedBy  = userId.ToString(),
                Notes      = "Claim created"
            });

            await _claimRepository.AddAsync(claim);
            await _claimRepository.SaveChangesAsync();

            _logger.LogInformation("Claim {ClaimId} created for policy {PolicyId} by user {UserId}",
                claim.ClaimId, dto.PolicyId, userId);

            return MapToDto(claim);
        }

        /// <summary>
        /// Performs the GetClaimByIdAsync operation.
        /// </summary>
        public async Task<ClaimResponseDTO?> GetClaimByIdAsync(Guid claimId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            return claim == null ? null : MapToDto(claim);
        }

        /// <summary>
        /// Performs the GetUserClaimsAsync operation.
        /// </summary>
        public async Task<PagedResult<ClaimResponseDTO>> GetUserClaimsAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            var pagedClaims = await _claimRepository.GetByUserIdAsync(userId, page, pageSize);
            return new PagedResult<ClaimResponseDTO>
            {
                Page = pagedClaims.Page,
                PageSize = pagedClaims.PageSize,
                TotalCount = pagedClaims.TotalCount,
                Items = pagedClaims.Items.Select(MapToDto).ToList()
            };
        }

        /// <summary>
        /// Performs the GetAllClaimsAsync operation.
        /// </summary>
        public async Task<PagedResult<ClaimResponseDTO>> GetAllClaimsAsync(int page = 1, int pageSize = 10)
        {
            var pagedClaims = await _claimRepository.GetAllAsync(page, pageSize);
            return new PagedResult<ClaimResponseDTO>
            {
                Page = pagedClaims.Page,
                PageSize = pagedClaims.PageSize,
                TotalCount = pagedClaims.TotalCount,
                Items = pagedClaims.Items.Select(MapToDto).ToList()
            };
        }

        /// <summary>
        /// Performs the GetClaimsByPolicyAsync operation.
        /// </summary>
        public async Task<List<ClaimResponseDTO>> GetClaimsByPolicyAsync(Guid policyId)
        {
            var claims = await _claimRepository.GetByPolicyIdAsync(policyId);
            return claims.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Performs the UpdateClaimStatusAsync operation.
        /// </summary>
        public async Task UpdateClaimStatusAsync(Guid claimId, string newStatus, string? notes, string changedBy)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new NotFoundException("Claim", claimId);

            var oldStatus = claim.Status;
            claim.Status = newStatus;
            claim.UpdatedAt = DateTime.UtcNow;

            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                ClaimId = claimId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = changedBy,
                Notes = notes ?? $"Status changed to {newStatus}"
            });

            await _claimRepository.SaveChangesAsync();

            await _bus.Publish(new ClaimStatusChangedEvent(
                claimId, oldStatus, newStatus, changedBy, DateTime.UtcNow, claim.UserId));

            _logger.LogInformation("Claim {ClaimId} status changed from {OldStatus} to {NewStatus} by {ChangedBy}",
                claimId, oldStatus, newStatus, changedBy);

            // Get user details before spawning task as background thread is outside the request context
            var (email, fullName) = await GetUserDetailsAsync(claim.UserId);

            // Send email notification asynchronously (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        if (newStatus == ClaimStatus.UnderReview)
                        {
                            await _emailService.SendClaimUnderReviewEmailAsync(email, fullName, claimId.ToString());
                        }
                        else if (newStatus == ClaimStatus.Closed)
                        {
                            await _emailService.SendClaimClosedEmailAsync(email, fullName, claimId.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send status change email for claim {ClaimId}", claimId);
                }
            });
        }

        /// <summary>
        /// Performs the UpdateClaimAsync operation.
        /// </summary>
        public async Task<ClaimResponseDTO> UpdateClaimAsync(Guid claimId, UpdateClaimDTO dto)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new NotFoundException("Claim", claimId);

            if (claim.Status != ClaimStatus.Draft)
                throw new BusinessRuleException("Only draft claims can be updated.");

            if (!string.IsNullOrEmpty(dto.Description))
                claim.Description = dto.Description;

            if (dto.ClaimAmount.HasValue)
                claim.ClaimAmount = dto.ClaimAmount.Value;

            claim.UpdatedAt = DateTime.UtcNow;
            await _claimRepository.SaveChangesAsync();

            _logger.LogInformation("Claim {ClaimId} updated", claimId);
            return MapToDto(claim);
        }

        /// <summary>
        /// Submits a draft claim for administration review. 
        /// Transitions the claim to 'Submitted' status and fires necessary domain events.
        /// </summary>
        public async Task SubmitClaimAsync(Guid claimId, Guid userId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new NotFoundException("Claim", claimId);
            if (claim.Status != ClaimStatus.Draft)
                throw new BusinessRuleException("Only draft claims can be submitted.");

            var oldStatus  = claim.Status;
            claim.Status    = ClaimStatus.Submitted;
            claim.UpdatedAt = DateTime.UtcNow;

            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                ClaimId   = claimId,
                OldStatus = oldStatus,
                NewStatus = ClaimStatus.Submitted,
                ChangedBy = userId.ToString(),
                Notes     = "Claim submitted for review"
            });

            await _claimRepository.SaveChangesAsync();

            await _bus.Publish(new ClaimSubmittedEvent(
                claimId, claim.PolicyId, claim.UserId, claim.Description, DateTime.UtcNow));

            await _bus.Publish(new ClaimStatusChangedEvent(
                claimId, oldStatus, ClaimStatus.Submitted, userId.ToString(), DateTime.UtcNow, claim.UserId));

            _logger.LogInformation("Claim {ClaimId} submitted", claimId);
        }

        /// <summary>
        /// Performs the GetClaimHistoryAsync operation.
        /// </summary>
        public async Task<List<ClaimStatusHistoryDTO>> GetClaimHistoryAsync(Guid claimId)
        {
            var history = await _historyRepository.GetByClaimIdAsync(claimId);

            return history.Select(h => new ClaimStatusHistoryDTO
            {
                Id        = h.Id,
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                Notes     = h.Notes,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt
            }).ToList();
        }

        /// <summary>
        /// Approves a claim, potentially adjusting the approved amount (e.g., matching the IDV for theft).
        /// Increments the policy's accident count or triggers policy termination if completely damaged/stolen.
        /// </summary>
        public async Task ApproveClaimAsync(Guid claimId, decimal approvedAmount, string? notes, string adminId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new NotFoundException("Claim", claimId);

            // For theft claims, automatically set approved amount to full IDV
            if (claim.ClaimType == "Stolen")
            {
                try
                {
                    var policyIdv = await GetPolicyIDVAsync(claim.PolicyId);
                    if (policyIdv > 0)
                    {
                        approvedAmount = policyIdv;
                        _logger.LogInformation("Theft claim {ClaimId} - Setting approved amount to full IDV: {IDV}", claimId, policyIdv);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch policy IDV for theft claim {ClaimId}, using provided amount", claimId);
                }
            }

            claim.Status         = ClaimStatus.Approved;
            claim.ApprovedAmount = approvedAmount;
            claim.UpdatedAt      = DateTime.UtcNow;

            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                ClaimId   = claimId,
                OldStatus = ClaimStatus.Submitted,
                NewStatus = ClaimStatus.Approved,
                ChangedBy = adminId,
                Notes     = notes ?? "Approved by administrator"
            });

            await _claimRepository.SaveChangesAsync();

            // Increment approved claims count in policy
            _ = Task.Run(async () =>
            {
                try
                {
                    await IncrementPolicyClaimCountAsync(claim.PolicyId);
                    _logger.LogInformation("Incremented approved claims count for policy {PolicyId}", claim.PolicyId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to increment claim count for policy {PolicyId}", claim.PolicyId);
                }
            });

            // If theft claim, terminate the policy
            if (claim.ClaimType == "Stolen")
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await TerminatePolicyAsync(claim.PolicyId);
                        _logger.LogInformation("Policy {PolicyId} terminated due to theft claim {ClaimId}", claim.PolicyId, claimId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to terminate policy {PolicyId} for theft claim {ClaimId}", claim.PolicyId, claimId);
                    }
                });
            }

            await _bus.Publish(new ClaimStatusChangedEvent(
                claimId, ClaimStatus.Submitted, ClaimStatus.Approved, adminId, DateTime.UtcNow, claim.UserId));

            // Get user details before spawning task as background thread is outside the request context
            var (email, fullName) = await GetUserDetailsAsync(claim.UserId);

            // Send email notification asynchronously (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        await _emailService.SendClaimApprovedEmailAsync(email, fullName, claimId.ToString(), approvedAmount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send approval email for claim {ClaimId}", claimId);
                }
            });
        }

        /// <summary>
        /// Performs the RejectClaimAsync operation.
        /// </summary>
        public async Task RejectClaimAsync(Guid claimId, string reason, string adminId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new NotFoundException("Claim", claimId);

            claim.Status          = ClaimStatus.Rejected;
            claim.RejectionReason = reason;
            claim.UpdatedAt       = DateTime.UtcNow;

            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                ClaimId   = claimId,
                OldStatus = ClaimStatus.Submitted,
                NewStatus = ClaimStatus.Rejected,
                ChangedBy = adminId,
                Notes     = reason
            });

            await _claimRepository.SaveChangesAsync();

            await _bus.Publish(new ClaimStatusChangedEvent(
                claimId, ClaimStatus.Submitted, ClaimStatus.Rejected, adminId, DateTime.UtcNow, claim.UserId, reason));

            // Get user details before spawning task as background thread is outside the request context
            var (email, fullName) = await GetUserDetailsAsync(claim.UserId);

            // Send email notification asynchronously (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        await _emailService.SendClaimRejectedEmailAsync(email, fullName, claimId.ToString(), reason);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send rejection email for claim {ClaimId}", claimId);
                }
            });
        }

        /// <summary>
        /// Performs the TransitionStatusAsync operation.
        /// </summary>
        public async Task TransitionStatusAsync(Guid claimId, string newStatus, string changedBy, string? notes = null)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new NotFoundException("Claim", claimId);

            var oldStatus   = claim.Status;
            claim.Status    = newStatus;
            claim.UpdatedAt = DateTime.UtcNow;

            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                ClaimId   = claimId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = changedBy,
                Notes     = notes
            });

            await _claimRepository.SaveChangesAsync();

            await _bus.Publish(new ClaimStatusChangedEvent(
                claimId, oldStatus, newStatus, changedBy, DateTime.UtcNow, claim.UserId));

            _logger.LogInformation("Claim {ClaimId} status changed from {Old} to {New}", claimId, oldStatus, newStatus);
        }

        /// <summary>
        /// Maps a domain <see cref="Claim"/> entity to a detailed <see cref="ClaimResponseDTO"/>.
        /// </summary>
        private static ClaimResponseDTO MapToDto(Claim claim)
        {
            return new ClaimResponseDTO
            {
                ClaimId             = claim.ClaimId,
                PolicyId            = claim.PolicyId,
                UserId              = claim.UserId,
                Description         = claim.Description,
                Status              = claim.Status,
                ClaimAmount         = claim.ClaimAmount,
                ApprovedAmount      = claim.ApprovedAmount,
                RejectionReason     = claim.RejectionReason,
                ClaimType           = claim.ClaimType,
                IsCompletelyDamaged = claim.IsCompletelyDamaged,
                CreatedAt           = claim.CreatedAt,
                UpdatedAt           = claim.UpdatedAt,
                Documents           = claim.Documents?.Select(d => new DocumentResponseDTO
                {
                    DocumentId  = d.DocumentId,
                    ClaimId     = d.ClaimId,
                    FileName    = d.FileName,
                    FileUrl     = d.FileUrl,
                    ContentType = d.ContentType,
                    FileSize    = d.FileSize,
                    UploadedAt  = d.UploadedAt
                }).ToList() ?? new()
            };
        }

        private async Task<decimal> GetPolicyIDVAsync(Guid policyId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";
                var response = await client.GetAsync($"{gatewayUrl}/policies/{policyId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
                    
                    if (jsonDoc.RootElement.TryGetProperty("insuredDeclaredValue", out var idvElement))
                    {
                        return idvElement.GetDecimal();
                    }
                }
                
                _logger.LogWarning("Failed to fetch policy IDV for policy {PolicyId}", policyId);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching policy IDV for policy {PolicyId}", policyId);
                return 0;
            }
        }

        private async Task TerminatePolicyAsync(Guid policyId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";
                
                // Call policy service to mark policy as terminated
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { isTerminated = true }),
                    System.Text.Encoding.UTF8,
                    "application/json");
                
                var response = await client.PatchAsync($"{gatewayUrl}/policies/{policyId}/terminate", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to terminate policy {PolicyId}. Status: {StatusCode}", 
                        policyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating policy {PolicyId}", policyId);
                throw;
            }
        }

        private async Task IncrementPolicyClaimCountAsync(Guid policyId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";
                
                // Call policy service to increment approved claims count
                var response = await client.PatchAsync($"{gatewayUrl}/policies/{policyId}/increment-claim-count", null);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to increment claim count for policy {PolicyId}. Status: {StatusCode}", 
                        policyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing claim count for policy {PolicyId}", policyId);
                throw;
            }
        }
    }
}
