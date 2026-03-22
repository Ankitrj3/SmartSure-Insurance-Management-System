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

        public PolicyMgmtService(IPolicyRepository repo, IInsuranceRepository insuranceRepo, IBus bus)
        {
            _repo = repo;
            _insuranceRepo = insuranceRepo;
            _bus = bus;
        }

        public async Task<List<PolicyDTO>> GetUserPoliciesAsync(Guid userId)
        {
            var policies = await _repo.GetByUserIdAsync(userId);
            return policies.Select(p => new PolicyDTO
            {
                PolicyId = p.PolicyId,
                UserId = p.UserId,
                SubtypeId = p.SubtypeId,
                SubtypeName = p.Subtype?.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PremiumAmount = p.PremiumAmount,
                Status = p.Status
            }).ToList();
        }

        public async Task<List<PolicyDTO>> GetAllPoliciesAsync()
        {
            var policies = await _repo.GetAllAsync();
            return policies.Select(p => new PolicyDTO
            {
                PolicyId = p.PolicyId,
                UserId = p.UserId,
                SubtypeId = p.SubtypeId,
                SubtypeName = p.Subtype?.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PremiumAmount = p.PremiumAmount,
                Status = p.Status
            }).ToList();
        }

        public async Task<PolicyDTO> GetPolicyByIdAsync(Guid policyId)
        {
            var p = await _repo.GetByIdAsync(policyId);
            if (p == null) return null;
            return new PolicyDTO
            {
                PolicyId = p.PolicyId,
                UserId = p.UserId,
                SubtypeId = p.SubtypeId,
                SubtypeName = p.Subtype?.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PremiumAmount = p.PremiumAmount,
                Status = p.Status
            };
        }

        public async Task<PolicyDTO> CreatePolicyAsync(Guid userId, CreatePolicyDTO dto)
        {
            var subtype = await _insuranceRepo.GetSubtypeByIdAsync(dto.SubtypeId);
            if (subtype == null) throw new Exception("Insurance subtype not found");

            var policy = new Policy
            {
                PolicyId = Guid.NewGuid(),
                UserId = userId,
                SubtypeId = dto.SubtypeId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(dto.Duration),
                PremiumAmount = subtype.BasePremium * dto.Duration, // Simple calculation
                Status = "Active"
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

            await _bus.Publish(new PolicyActivatedEvent(policy.PolicyId, policy.UserId, subtype.TypeId, policy.SubtypeId, DateTime.UtcNow));

            return new PolicyDTO
            {
                PolicyId = policy.PolicyId,
                UserId = policy.UserId,
                SubtypeId = policy.SubtypeId,
                SubtypeName = subtype.Name,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                PremiumAmount = policy.PremiumAmount,
                Status = policy.Status
            };
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
    }
}
