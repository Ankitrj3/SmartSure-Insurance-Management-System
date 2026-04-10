using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using SmartSure.Shared.Contracts.DTOs;
using MassTransit;
using SmartSure.Shared.Contracts.Events;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Represent or implements PolicyMgmtService.
    /// </summary>
    public class PolicyMgmtService : IPolicyMgmtService
    {
        private readonly IPolicyRepository _repo;
        private readonly IInsuranceRepository _insuranceRepo;
        private readonly IBus _bus;
        private readonly IDiscountService _discountService;
        private readonly ILogger<PolicyMgmtService> _logger;

        public PolicyMgmtService(
            IPolicyRepository repo,
            IInsuranceRepository insuranceRepo,
            IBus bus,
            IDiscountService discountService,
            ILogger<PolicyMgmtService> logger)
        {
            _repo            = repo;
            _insuranceRepo   = insuranceRepo;
            _bus             = bus;
            _discountService = discountService;
            _logger          = logger;
        }

        // ── Helpers: IDV / Insurance Value Calculation ─────────────────────────

        /// <summary>
        /// Vehicle IDV = Ex-showroom price × depreciation factor based on vehicle age.
        /// Standard IRDAI depreciation schedule:
        ///   ≤ 6 months  → 5%
        ///   6–12 months → 15%
        ///   1–2 years   → 20%
        ///   2–3 years   → 30%
        ///   3–4 years   → 40%
        ///   4–5 years   → 50%
        ///   > 5 years   → IDV agreed upon (we use 60% here)
        /// </summary>
        private static decimal CalculateVehicleIdv(PolicyVehicleDetailDTO v)
        {
            int currentYear = DateTime.UtcNow.Year;
            int age = currentYear - v.ManufactureYear;

            decimal depreciationRate = age switch
            {
                <= 0 => 0.05m,
                1    => 0.15m,
                2    => 0.20m,
                3    => 0.30m,
                4    => 0.40m,
                5    => 0.50m,
                _    => 0.60m
            };

            decimal idv = v.EstimatedValue * (1 - depreciationRate);
            return Math.Max(idv, 10000); // Minimum IDV ₹10,000
        }

        /// <summary>
        /// Home IDV = Area (sq.ft) × Reconstruction cost per sq.ft.
        /// Land value is NOT included. Only rebuilding cost is considered.
        /// </summary>
        private static decimal CalculateHomeInsuranceValue(PolicyHomeDetailDTO h)
        {
            decimal idv = h.AreaSqFt * h.ConstructionCostPerSqFt;
            return Math.Max(idv, 50000); // Minimum insured value ₹50,000
        }

        /// <summary>
        /// Premium = BasePremium × (duration / 12) + 18% GST (9% CGST + 9% SGST)
        /// </summary>
        private static decimal CalculatePremium(decimal basePremium, int durationMonths, decimal idv, bool isVehicle)
        {
            decimal years = durationMonths / 12.0m;
            decimal baseAmount = basePremium * years;
            
            if (idv > 0)
            {
                if (isVehicle)
                {
                    baseAmount += (idv * 0.025m * years);
                }
                else
                {
                    baseAmount += (idv * 0.005m * years); // 0.5% for home
                }
            }
            
            // Add 18% GST
            decimal premiumWithGst = baseAmount * 1.18m;
            return Math.Round(premiumWithGst, 2);
        }

        // ── Quote ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs the CalculateQuoteAsync operation.
        /// </summary>
        public async Task<PolicyQuoteDTO> CalculateQuoteAsync(CreatePolicyDTO dto)
        {
            var subtype = await _insuranceRepo.GetSubtypeByIdAsync(dto.SubtypeId);
            if (subtype == null) throw new NotFoundException("Insurance subtype", dto.SubtypeId);

            var type      = await _insuranceRepo.GetTypeByIdAsync(subtype.TypeId);
            bool isVehicle = type?.Name?.Contains("Vehicle", StringComparison.OrdinalIgnoreCase) == true;
            bool isHome    = type?.Name?.Contains("Home",    StringComparison.OrdinalIgnoreCase) == true;

            decimal idv       = 0;
            string breakdown  = "";

            if (isVehicle && dto.VehicleDetail != null)
            {
                idv = CalculateVehicleIdv(dto.VehicleDetail);
                int age   = DateTime.UtcNow.Year - dto.VehicleDetail.ManufactureYear;
                breakdown = $"Vehicle IDV Calculation: Ex-showroom ₹{dto.VehicleDetail.EstimatedValue:N0} | Age {age} yrs | IDV ₹{idv:N0}";
            }
            else if (isHome && dto.HomeDetail != null)
            {
                idv = CalculateHomeInsuranceValue(dto.HomeDetail);
                breakdown = $"Home IDV: {dto.HomeDetail.AreaSqFt:N0} sq.ft × ₹{dto.HomeDetail.ConstructionCostPerSqFt:N0}/sq.ft = ₹{idv:N0} (reconstruction cost, land excluded)";
            }

            decimal premium = CalculatePremium(subtype.BasePremium, dto.Duration, idv, isVehicle);

            _logger.LogInformation("Quote calculated: BasePremium=₹{BasePremium}, Duration={Duration} months, IDV=₹{IDV}, FinalPremium=₹{Premium}", 
                subtype.BasePremium, dto.Duration, idv, premium);

            return new PolicyQuoteDTO
            {
                SubtypeId            = subtype.SubtypeId,
                SubtypeName          = subtype.Name,
                TypeName             = type?.Name ?? "Unknown",
                Duration             = dto.Duration,
                InsuredDeclaredValue = idv,
                PremiumAmount        = premium,
                Breakdown            = breakdown
            };
        }

        // ── CRUD ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs the GetUserPoliciesAsync operation.
        /// </summary>
        public async Task<PagedResult<PolicyDTO>> GetUserPoliciesAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            var pagedPolicies = await _repo.GetByUserIdAsync(userId, page, pageSize);
            return new PagedResult<PolicyDTO>
            {
                Page = pagedPolicies.Page,
                PageSize = pagedPolicies.PageSize,
                TotalCount = pagedPolicies.TotalCount,
                Items = pagedPolicies.Items.Select(MapToDto).ToList()
            };
        }

        /// <summary>
        /// Performs the GetAllPoliciesAsync operation.
        /// </summary>
        public async Task<PagedResult<PolicyDTO>> GetAllPoliciesAsync(int page = 1, int pageSize = 10)
        {
            var pagedPolicies = await _repo.GetAllAsync(page, pageSize);
            return new PagedResult<PolicyDTO>
            {
                Page = pagedPolicies.Page,
                PageSize = pagedPolicies.PageSize,
                TotalCount = pagedPolicies.TotalCount,
                Items = pagedPolicies.Items.Select(MapToDto).ToList()
            };
        }

        /// <summary>
        /// Performs the GetPolicyByIdAsync operation.
        /// </summary>
        public async Task<PolicyDTO> GetPolicyByIdAsync(Guid policyId)
        {
            var p = await _repo.GetByIdAsync(policyId);
            if (p == null) return null!;
            return MapToDto(p);
        }

        /// <summary>
        /// Performs the CreatePolicyAsync operation.
        /// </summary>
        public async Task<PolicyDTO> CreatePolicyAsync(Guid userId, CreatePolicyDTO dto)
        {
            var subtype = await _insuranceRepo.GetSubtypeByIdAsync(dto.SubtypeId);
            if (subtype == null) throw new NotFoundException("Insurance subtype", dto.SubtypeId);

            var type      = await _insuranceRepo.GetTypeByIdAsync(subtype.TypeId);
            bool isVehicle = type?.Name?.Contains("Vehicle", StringComparison.OrdinalIgnoreCase) == true;
            bool isHome    = type?.Name?.Contains("Home",    StringComparison.OrdinalIgnoreCase) == true;

            // Calculate IDV
            decimal idv = 0;
            if (isVehicle && dto.VehicleDetail != null)
                idv = CalculateVehicleIdv(dto.VehicleDetail);
            else if (isHome && dto.HomeDetail != null)
                idv = CalculateHomeInsuranceValue(dto.HomeDetail);

            // Calculate premium based on IDV
            decimal basePremium = CalculatePremium(subtype.BasePremium, dto.Duration, idv, isVehicle);

            // Apply discounts if applicable
            var discountResult = await _discountService.CalculateDiscountAsync(userId, basePremium, dto.CouponCode);
            decimal finalPremium = discountResult.FinalPremium;

            var policy = new Policy
            {
                PolicyId             = Guid.NewGuid(),
                UserId               = userId,
                SubtypeId            = dto.SubtypeId,
                StartDate            = DateTime.UtcNow,
                EndDate              = DateTime.UtcNow.AddMonths(dto.Duration),
                PremiumAmount        = finalPremium,
                InsuredDeclaredValue = idv,
                Status               = "Pending",
                NomineeName          = dto.NomineeName,
                NomineeRelation      = dto.NomineeRelation
            };

            await _repo.AddAsync(policy);

            if (dto.HomeDetail != null)
            {
                await _repo.AddOrUpdateHomeDetailAsync(new HomeDetail
                {
                    HomeDetailId              = Guid.NewGuid(),
                    PolicyId                  = policy.PolicyId,
                    Address                   = dto.HomeDetail.Address,
                    PropertyType              = dto.HomeDetail.PropertyType,
                    YearBuilt                 = dto.HomeDetail.YearBuilt,
                    AreaSqFt                  = dto.HomeDetail.AreaSqFt,
                    ConstructionCostPerSqFt   = dto.HomeDetail.ConstructionCostPerSqFt,
                    EstimatedValue            = dto.HomeDetail.AreaSqFt * dto.HomeDetail.ConstructionCostPerSqFt,
                    SecurityFeatures          = dto.HomeDetail.SecurityFeatures
                });
            }

            if (dto.VehicleDetail != null)
            {
                await _repo.AddOrUpdateVehicleDetailAsync(new VehicleDetail
                {
                    VehicleDetailId    = Guid.NewGuid(),
                    PolicyId           = policy.PolicyId,
                    RegistrationNumber = dto.VehicleDetail.RegistrationNumber,
                    Make               = dto.VehicleDetail.Make,
                    Model              = dto.VehicleDetail.Model,
                    ManufactureYear    = dto.VehicleDetail.ManufactureYear,
                    EstimatedValue     = dto.VehicleDetail.EstimatedValue,
                    ChassisNumber      = dto.VehicleDetail.ChassisNumber,
                    EngineNumber       = dto.VehicleDetail.EngineNumber
                });
            }

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Policy {PolicyId} created for user {UserId} with IDV ₹{IDV} and Premium ₹{Premium}",
                policy.PolicyId, userId, idv, policy.PremiumAmount);

            return new PolicyDTO
            {
                PolicyId             = policy.PolicyId,
                UserId               = policy.UserId,
                SubtypeId            = policy.SubtypeId,
                SubtypeName          = subtype.Name,
                TypeName             = type?.Name,
                StartDate            = policy.StartDate,
                EndDate              = policy.EndDate,
                PremiumAmount        = policy.PremiumAmount,
                InsuredDeclaredValue = policy.InsuredDeclaredValue,
                Status               = policy.Status,
                NomineeName          = policy.NomineeName,
                NomineeRelation      = policy.NomineeRelation
            };
        }

        /// <summary>
        /// Performs the ActivatePolicyAsync operation.
        /// </summary>
        public async Task ActivatePolicyAsync(Guid policyId)
        {
            var policy = await _repo.GetByIdAsync(policyId);
            if (policy == null) throw new NotFoundException("Policy", policyId);
            if (policy.Status == "Active") return; // Already active

            policy.Status = "Active";
            await _repo.UpdateAsync(policy);
            await _repo.SaveChangesAsync();

            await _bus.Publish(new PolicyActivatedEvent(
                policy.PolicyId, policy.UserId,
                policy.Subtype?.TypeId ?? Guid.Empty,
                policy.SubtypeId, DateTime.UtcNow));

            _logger.LogInformation("Policy {PolicyId} activated after payment", policyId);
        }

        /// <summary>
        /// Performs the CancelPolicyAsync operation.
        /// </summary>
        public async Task CancelPolicyAsync(Guid policyId)
        {
            var policy = await _repo.GetByIdAsync(policyId);
            if (policy == null) return;

            await _repo.CancelAsync(policyId);
            await _bus.Publish(new PolicyCancelledEvent(policyId, policy.UserId, "Cancelled by user", DateTime.UtcNow));
        }

        /// <summary>
        /// Performs the FailPolicyAsync operation.
        /// </summary>
        public async Task FailPolicyAsync(Guid policyId)
        {
            var policy = await _repo.GetByIdAsync(policyId);
            if (policy == null) return;

            // Create a failed payment record so that it appears in transaction history.
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                PolicyId = policy.PolicyId,
                Amount = policy.PremiumAmount,
                PaymentDate = DateTime.UtcNow,
                Status = "Failed",
                PaymentMethod = "Unknown", // Can be specified if known
                TransactionReference = "FAILED_" + Guid.NewGuid().ToString().Substring(0, 8)
            };

            await _repo.AddPaymentAsync(payment);
            
            policy.Status = "Failed";
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Performs the DeletePolicyAsync operation.
        /// </summary>
        public async Task DeletePolicyAsync(Guid policyId)
        {
            var policy = await _repo.GetByIdAsync(policyId);
            if (policy == null) return;

            await _repo.DeleteAsync(policyId);
            // Optionally publish an event, but since it's unpurchased it's fine.
        }

        /// <summary>
        /// Performs the GetPolicyDetailsAsync operation.
        /// </summary>
        public async Task<PolicyDetailDTO> GetPolicyDetailsAsync(Guid policyId)
        {
            var detail = await _repo.GetDetailByPolicyIdAsync(policyId);
            if (detail == null) return null!;
            return new PolicyDetailDTO
            {
                PolicyId           = detail.PolicyId,
                TermsAndConditions = detail.TermsAndConditions,
                Inclusions         = detail.Inclusions,
                Exclusions         = detail.Exclusions
            };
        }

        /// <summary>
        /// Performs the SavePolicyDetailsAsync operation.
        /// </summary>
        public async Task SavePolicyDetailsAsync(Guid policyId, SavePolicyDetailDTO dto)
        {
            var detail = new PolicyDetail
            {
                DocumentId         = Guid.NewGuid(),
                PolicyId           = policyId,
                TermsAndConditions = dto.TermsAndConditions,
                Inclusions         = dto.Inclusions,
                Exclusions         = dto.Exclusions
            };
            await _repo.AddOrUpdateDetailAsync(detail);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Performs the GetPremiumAmountAsync operation.
        /// </summary>
        public async Task<decimal> GetPremiumAmountAsync(Guid policyId)
        {
            var p = await _repo.GetByIdAsync(policyId);
            if (p == null) throw new NotFoundException("Policy", policyId);
            return p.PremiumAmount;
        }

        /// <summary>
        /// Performs the GetHomeDetailAsync operation.
        /// </summary>
        public async Task<CreateHomeDetailDTO> GetHomeDetailAsync(Guid policyId)
        {
            var detail = await _repo.GetHomeDetailByPolicyIdAsync(policyId);
            if (detail == null) return null!;
            return new CreateHomeDetailDTO
            {
                Address          = detail.Address,
                PropertyType     = detail.PropertyType,
                YearBuilt        = detail.YearBuilt,
                EstimatedValue   = detail.EstimatedValue,
                SecurityFeatures = detail.SecurityFeatures
            };
        }

        /// <summary>
        /// Performs the SaveHomeDetailAsync operation.
        /// </summary>
        public async Task SaveHomeDetailAsync(Guid policyId, CreateHomeDetailDTO dto)
        {
            var detail = new HomeDetail
            {
                PolicyId         = policyId,
                Address          = dto.Address,
                PropertyType     = dto.PropertyType,
                YearBuilt        = dto.YearBuilt,
                EstimatedValue   = dto.EstimatedValue,
                SecurityFeatures = dto.SecurityFeatures
            };
            await _repo.AddOrUpdateHomeDetailAsync(detail);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Performs the GetVehicleDetailAsync operation.
        /// </summary>
        public async Task<CreateVehicleDetailDTO> GetVehicleDetailAsync(Guid policyId)
        {
            var detail = await _repo.GetVehicleDetailByPolicyIdAsync(policyId);
            if (detail == null) return null!;
            return new CreateVehicleDetailDTO
            {
                RegistrationNumber = detail.RegistrationNumber,
                Make               = detail.Make,
                Model              = detail.Model,
                ManufactureYear    = detail.ManufactureYear,
                EstimatedValue     = detail.EstimatedValue,
                ChassisNumber      = detail.ChassisNumber,
                EngineNumber       = detail.EngineNumber
            };
        }

        /// <summary>
        /// Performs the SaveVehicleDetailAsync operation.
        /// </summary>
        public async Task SaveVehicleDetailAsync(Guid policyId, CreateVehicleDetailDTO dto)
        {
            var detail = new VehicleDetail
            {
                PolicyId           = policyId,
                RegistrationNumber = dto.RegistrationNumber,
                Make               = dto.Make,
                Model              = dto.Model,
                ManufactureYear    = dto.ManufactureYear,
                EstimatedValue     = dto.EstimatedValue,
                ChassisNumber      = dto.ChassisNumber,
                EngineNumber       = dto.EngineNumber
            };
            await _repo.AddOrUpdateVehicleDetailAsync(detail);
            await _repo.SaveChangesAsync();
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static PolicyDTO MapToDto(Policy p)
        {
            return new PolicyDTO
            {
                PolicyId             = p.PolicyId,
                UserId               = p.UserId,
                SubtypeId            = p.SubtypeId,
                SubtypeName          = p.Subtype?.Name,
                TypeName             = p.Subtype?.Type?.Name,
                StartDate            = p.StartDate,
                EndDate              = p.EndDate,
                PremiumAmount        = p.PremiumAmount,
                InsuredDeclaredValue = p.InsuredDeclaredValue,
                Status               = p.Status,
                NomineeName          = p.NomineeName,
                NomineeRelation      = p.NomineeRelation
            };
        }

        /// <summary>
        /// Performs the TerminatePolicyAsync operation.
        /// </summary>
        public async Task TerminatePolicyAsync(Guid policyId)
        {
            var policy = await _repo.GetByIdAsync(policyId);
            if (policy == null)
                throw new NotFoundException("Policy", policyId);

            policy.IsTerminated = true;
            policy.Status = "Cancelled";
            
            await _repo.SaveChangesAsync();
            
            _logger.LogInformation("Policy {PolicyId} has been terminated", policyId);
        }

        /// <summary>
        /// Performs the IncrementApprovedClaimsCountAsync operation.
        /// </summary>
        public async Task IncrementApprovedClaimsCountAsync(Guid policyId)
        {
            var policy = await _repo.GetByIdAsync(policyId);
            if (policy == null)
                throw new NotFoundException("Policy", policyId);

            policy.ApprovedClaimsCount++;
            
            await _repo.SaveChangesAsync();
            
            _logger.LogInformation("Policy {PolicyId} approved claims count incremented to {Count}", 
                policyId, policy.ApprovedClaimsCount);
        }
    }
}
