using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly PolicyDbContext _context;

        public PolicyRepository(PolicyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Policy>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Policies
                .Include(p => p.Subtype)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetAllAsync()
        {
            return await _context.Policies
                .Include(p => p.Subtype)
                .ToListAsync();
        }

        public async Task<Policy> GetByIdAsync(Guid policyId)
        {
            return await _context.Policies
                .Include(p => p.Subtype)
                .Include(p => p.PolicyDetail)
                .Include(p => p.HomeDetail)
                .Include(p => p.VehicleDetail)
                .FirstOrDefaultAsync(p => p.PolicyId == policyId);
        }

        public async Task AddAsync(Policy policy)
        {
            await _context.Policies.AddAsync(policy);
        }

        public async Task UpdateAsync(Policy policy)
        {
            _context.Policies.Update(policy);
        }

        public async Task CancelAsync(Guid policyId)
        {
            var policy = await _context.Policies.FindAsync(policyId);
            if (policy != null)
            {
                policy.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PolicyDetail> GetDetailByPolicyIdAsync(Guid policyId)
        {
            return await _context.PolicyDetails.FirstOrDefaultAsync(pd => pd.PolicyId == policyId);
        }

        public async Task AddOrUpdateDetailAsync(PolicyDetail detail)
        {
            var existing = await _context.PolicyDetails.FirstOrDefaultAsync(pd => pd.PolicyId == detail.PolicyId);
            if (existing == null)
            {
                await _context.PolicyDetails.AddAsync(detail);
            }
            else
            {
                existing.TermsAndConditions = detail.TermsAndConditions;
                existing.Inclusions = detail.Inclusions;
                existing.Exclusions = detail.Exclusions;
            }
        }

        public async Task<HomeDetail> GetHomeDetailByPolicyIdAsync(Guid policyId)
        {
            return await _context.HomeDetails.FirstOrDefaultAsync(hd => hd.PolicyId == policyId);
        }

        public async Task AddOrUpdateHomeDetailAsync(HomeDetail detail)
        {
            var existing = await _context.HomeDetails.FirstOrDefaultAsync(hd => hd.PolicyId == detail.PolicyId);
            if (existing == null)
            {
                await _context.HomeDetails.AddAsync(detail);
            }
            else
            {
                existing.Address = detail.Address;
                existing.PropertyType = detail.PropertyType;
                existing.YearBuilt = detail.YearBuilt;
                existing.EstimatedValue = detail.EstimatedValue;
                existing.SecurityFeatures = detail.SecurityFeatures;
            }
        }

        public async Task<VehicleDetail> GetVehicleDetailByPolicyIdAsync(Guid policyId)
        {
            return await _context.VehicleDetails.FirstOrDefaultAsync(vd => vd.PolicyId == policyId);
        }

        public async Task AddOrUpdateVehicleDetailAsync(VehicleDetail detail)
        {
            var existing = await _context.VehicleDetails.FirstOrDefaultAsync(vd => vd.PolicyId == detail.PolicyId);
            if (existing == null)
            {
                await _context.VehicleDetails.AddAsync(detail);
            }
            else
            {
                existing.RegistrationNumber = detail.RegistrationNumber;
                existing.Make = detail.Make;
                existing.Model = detail.Model;
                existing.ManufactureYear = detail.ManufactureYear;
                existing.EstimatedValue = detail.EstimatedValue;
                existing.ChassisNumber = detail.ChassisNumber;
                existing.EngineNumber = detail.EngineNumber;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
