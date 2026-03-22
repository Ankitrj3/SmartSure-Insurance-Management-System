using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using MassTransit;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.PolicyService.Services
{
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
            _repo = repo;
            _insuranceRepo = insuranceRepo;
            _bus = bus;
            _discountService = discountService;
            _logger = logger;
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
        private static decimal CalculateVehicleIdv(CreateVehicleDetailDTO v)
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
            return Math.Max(idv, 10000); // Minimum IDV of ₹10,000
        }

        /// <summary>
        /// Home Insurance Value = Rebuild cost estimation.
        /// Formula: Current market value adjusted by age depreciation and property-type multiplier.
        ///   - Apartment   → 0.90 multiplier
        ///   - House       → 1.00 multiplier
        ///   - Villa       → 1.10 multiplier
        /// Age depreciation: 1% per year after construction, capped at 40%.
        /// </summary>
        private static decimal CalculateHomeInsuranceValue(CreateHomeDetailDTO h)
        {
            int currentYear = DateTime.UtcNow.Year;
            int age = currentYear - h.YearBuilt;
            if (age < 0) age = 0;

            decimal ageDepreciation = Math.Min(age * 0.01m, 0.40m);

            decimal propertyMultiplier = h.PropertyType?.ToLower() switch
            {
                "apartment" => 0.90m,
                "villa"     => 1.10m,
                _           => 1.00m  // House or default
            };

            decimal insuranceValue = h.EstimatedValue * (1 - ageDepreciation) * propertyMultiplier;
            return Math.Max(insuranceValue, 50000); // Minimum insured value ₹50,000
        }

        /// <summary>
        /// Premium = BasePremium × (duration / 12) + IDV-based risk component.
        /// Risk component: 2.5% of IDV per year for vehicles, 0.15% of insurance value per year for homes.
        /// </summary>
        private static decimal CalculatePremium(decimal basePremium, int durationMonths, decimal idv, bool isVehicle)
        {
            decimal years = durationMonths / 12.0m;
            decimal basePart = basePremium * years;
            decimal riskRate = isVehicle ? 0.025m : 0.0015m;
            decimal riskPart = idv * riskRate * years;
            decimal total = basePart + riskPart;
            return Math.Round(total, 2);
        }

        // ── Quote ──────────────────────────────────────────────────────────────

        public async Task<PolicyQuoteDTO> CalculateQuoteAsync(CreatePolicyDTO dto)
        {
            var subtype = await _insuranceRepo.GetSubtypeByIdAsync(dto.SubtypeId);
            if (subtype == null) throw new Exception("Insurance subtype not found");

            var type = await _insuranceRepo.GetTypeByIdAsync(subtype.TypeId);
            bool isVehicle = type?.Name?.Contains("Vehicle", StringComparison.OrdinalIgnoreCase) == true;
            bool isHome = type?.Name?.Contains("Home", StringComparison.OrdinalIgnoreCase) == true;

            decimal idv = 0;
            string breakdown = "";

            if (isVehicle && dto.VehicleDetail != null)
            {
                idv = CalculateVehicleIdv(dto.VehicleDetail);
                int age = DateTime.UtcNow.Year - dto.VehicleDetail.ManufactureYear;
                breakdown = $"Vehicle IDV Calculation: Ex-showroom ₹{dto.VehicleDetail.EstimatedValue:N0} | Age {age} yrs | IDV ₹{idv:N0}";
            }
            else if (isHome && dto.HomeDetail != null)
            {
                idv = CalculateHomeInsuranceValue(dto.HomeDetail);
                int age = DateTime.UtcNow.Year - dto.HomeDetail.YearBuilt;
                breakdown = $"Home Insurance Value: Market Value ₹{dto.HomeDetail.EstimatedValue:N0} | Age {age} yrs | Insured Value ₹{idv:N0}";
            }

            decimal premium = CalculatePremium(subtype.BasePremium, dto.Duration, idv, isVehicle);

            return new PolicyQuoteDTO
            {
                SubtypeId = subtype.SubtypeId,
                SubtypeName = subtype.Name,
                TypeName = type?.Name ?? "Unknown",
                Duration = dto.Duration,
                InsuredDeclaredValue = idv,
                PremiumAmount = premium,
                Breakdown = breakdown
            };
        }

        // ── CRUD ───────────────────────────────────────────────────────────────

        public async Task<List<PolicyDTO>> GetUserPoliciesAsync(Guid userId)
        {
            var policies = await _repo.GetByUserIdAsync(userId);
            return policies.Select(MapToDto).ToList();
        }

        public async Task<List<PolicyDTO>> GetAllPoliciesAsync()
        {
            var policies = await _repo.GetAllAsync();
            return policies.Select(MapToDto).ToList();
        }

        public async Task<PolicyDTO> GetPolicyByIdAsync(Guid policyId)
        {
            var p = await _repo.GetByIdAsync(policyId);
            if (p == null) return null;
            return MapToDto(p);
        }

        public async Task<PolicyDTO> CreatePolicyAsync(Guid userId, CreatePolicyDTO dto)
        {
            var subtype = await _insuranceRepo.GetSubtypeByIdAsync(dto.SubtypeId);
            if (subtype == null) throw new Exception("Insurance subtype not found");

            var type = await _insuranceRepo.GetTypeByIdAsync(subtype.TypeId);
            bool isVehicle = type?.Name?.Contains("Vehicle", StringComparison.OrdinalIgnoreCase) == true;
            bool isHome = type?.Name?.Contains("Home", StringComparison.OrdinalIgnoreCase) == true;

            // Calculate IDV
            decimal idv = 0;
            if (isVehicle && dto.VehicleDetail != null)
            {
                idv = CalculateVehicleIdv(dto.VehicleDetail);
            }
            else if (isHome && dto.HomeDetail != null)
            {
                idv = CalculateHomeInsuranceValue(dto.HomeDetail);
            }

            // Calculate premium based on IDV
            decimal basePremium = CalculatePremium(subtype.BasePremium, dto.Duration, idv, isVehicle);
            
            // Apply discounts if applicable
            var discountResult = await _discountService.CalculateDiscountAsync(userId, basePremium, dto.CouponCode);
            decimal finalPremium = discountResult.FinalPremium;

            var policy = new Policy
            {
                PolicyId = Guid.NewGuid(),
                UserId = userId,
                SubtypeId = dto.SubtypeId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(dto.Duration),
                PremiumAmount = finalPremium,
                InsuredDeclaredValue = idv,
                Status = "Pending" // Will become "Active" after payment
            };

            await _repo.AddAsync(policy);

            if (dto.HomeDetail != null)
            {
                await _repo.AddOrUpdateHomeDetailAsync(new HomeDetail
                {
                    HomeDetailId = Guid.NewGuid(),
                    PolicyId = policy.PolicyId,
                    Address = dto.HomeDetail.Address,
                    PropertyType = dto.HomeDetail.PropertyType,
                    YearBuilt = dto.HomeDetail.YearBuilt,
                    EstimatedValue = dto.HomeDetail.EstimatedValue,
                    SecurityFeatures = dto.HomeDetail.SecurityFeatures
                });
            }

            if (dto.VehicleDetail != null)
            {
                await _repo.AddOrUpdateVehicleDetailAsync(new VehicleDetail
                {
                    VehicleDetailId = Guid.NewGuid(),
                    PolicyId = policy.PolicyId,
                    RegistrationNumber = dto.VehicleDetail.RegistrationNumber,
                    Make = dto.VehicleDetail.Make,
                    Model = dto.VehicleDetail.Model,
                    ManufactureYear = dto.VehicleDetail.ManufactureYear,
                    EstimatedValue = dto.VehicleDetail.EstimatedValue,
                    ChassisNumber = dto.VehicleDetail.ChassisNumber,
                    EngineNumber = dto.VehicleDetail.EngineNumber
                });
            }

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Policy {PolicyId} created for user {UserId} with IDV ₹{IDV} and Premium ₹{Premium}",
                policy.PolicyId, userId, idv, policy.PremiumAmount);

            return new PolicyDTO
            {
                PolicyId = policy.PolicyId,
                UserId = policy.UserId,
                SubtypeId = policy.SubtypeId,
                SubtypeName = subtype.Name,
                TypeName = type?.Name,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                PremiumAmount = policy.PremiumAmount,
                InsuredDeclaredValue = policy.InsuredDeclaredValue,
                Status = policy.Status
            };
        }

        public async Task ActivatePolicyAsync(Guid policyId)
        {
            var policy = await _repo.GetByIdAsync(policyId);
            if (policy == null) throw new Exception("Policy not found");
            if (policy.Status == "Active") return; // Already active

            policy.Status = "Active";
            await _repo.UpdateAsync(policy);
            await _repo.SaveChangesAsync();

            // Publish event
            await _bus.Publish(new PolicyActivatedEvent(policy.PolicyId, policy.UserId, policy.Subtype?.TypeId ?? Guid.Empty, policy.SubtypeId, DateTime.UtcNow));

            _logger.LogInformation("Policy {PolicyId} activated after payment", policyId);
        }

        public async Task CancelPolicyAsync(Guid policyId)
        {
            var policy = await _repo.GetByIdAsync(policyId);
            if (policy == null) return;

            await _repo.CancelAsync(policyId);
            await _bus.Publish(new PolicyCancelledEvent(policyId, policy.UserId, "Cancelled by user", DateTime.UtcNow));
        }

        public async Task<PolicyDetailDTO> GetPolicyDetailsAsync(Guid policyId)
        {
            var detail = await _repo.GetDetailByPolicyIdAsync(policyId);
            if (detail == null) return null;
            return new PolicyDetailDTO
            {
                PolicyId = detail.PolicyId,
                TermsAndConditions = detail.TermsAndConditions,
                Inclusions = detail.Inclusions,
                Exclusions = detail.Exclusions
            };
        }

        public async Task SavePolicyDetailsAsync(Guid policyId, SavePolicyDetailDTO dto)
        {
            var detail = new PolicyDetail
            {
                DocumentId = Guid.NewGuid(),
                PolicyId = policyId,
                TermsAndConditions = dto.TermsAndConditions,
                Inclusions = dto.Inclusions,
                Exclusions = dto.Exclusions
            };
            await _repo.AddOrUpdateDetailAsync(detail);
            await _repo.SaveChangesAsync();
        }

        public async Task<decimal> GetPremiumAmountAsync(Guid policyId)
        {
            var p = await _repo.GetByIdAsync(policyId);
            if (p == null) throw new Exception("Policy not found");
            return p.PremiumAmount;
        }

        public async Task<CreateHomeDetailDTO> GetHomeDetailAsync(Guid policyId)
        {
            var detail = await _repo.GetHomeDetailByPolicyIdAsync(policyId);
            if (detail == null) return null;
            return new CreateHomeDetailDTO
            {
                Address = detail.Address,
                PropertyType = detail.PropertyType,
                YearBuilt = detail.YearBuilt,
                EstimatedValue = detail.EstimatedValue,
                SecurityFeatures = detail.SecurityFeatures
            };
        }

        public async Task SaveHomeDetailAsync(Guid policyId, CreateHomeDetailDTO dto)
        {
            var detail = new HomeDetail
            {
                PolicyId = policyId,
                Address = dto.Address,
                PropertyType = dto.PropertyType,
                YearBuilt = dto.YearBuilt,
                EstimatedValue = dto.EstimatedValue,
                SecurityFeatures = dto.SecurityFeatures
            };
            await _repo.AddOrUpdateHomeDetailAsync(detail);
            await _repo.SaveChangesAsync();
        }

        public async Task<CreateVehicleDetailDTO> GetVehicleDetailAsync(Guid policyId)
        {
            var detail = await _repo.GetVehicleDetailByPolicyIdAsync(policyId);
            if (detail == null) return null;
            return new CreateVehicleDetailDTO
            {
                RegistrationNumber = detail.RegistrationNumber,
                Make = detail.Make,
                Model = detail.Model,
                ManufactureYear = detail.ManufactureYear,
                EstimatedValue = detail.EstimatedValue,
                ChassisNumber = detail.ChassisNumber,
                EngineNumber = detail.EngineNumber
            };
        }

        public async Task SaveVehicleDetailAsync(Guid policyId, CreateVehicleDetailDTO dto)
        {
            var detail = new VehicleDetail
            {
                PolicyId = policyId,
                RegistrationNumber = dto.RegistrationNumber,
                Make = dto.Make,
                Model = dto.Model,
                ManufactureYear = dto.ManufactureYear,
                EstimatedValue = dto.EstimatedValue,
                ChassisNumber = dto.ChassisNumber,
                EngineNumber = dto.EngineNumber
            };
            await _repo.AddOrUpdateVehicleDetailAsync(detail);
            await _repo.SaveChangesAsync();
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private PolicyDTO MapToDto(Policy p)
        {
            return new PolicyDTO
            {
                PolicyId = p.PolicyId,
                UserId = p.UserId,
                SubtypeId = p.SubtypeId,
                SubtypeName = p.Subtype?.Name,
                TypeName = p.Subtype?.Type?.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PremiumAmount = p.PremiumAmount,
                InsuredDeclaredValue = p.InsuredDeclaredValue,
                Status = p.Status
            };
        }
    }
}
