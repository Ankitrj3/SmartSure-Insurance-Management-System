using Microsoft.EntityFrameworkCore;
using SmartSure.Shared.Contracts.Extensions;
using SmartSure.Shared.Contracts.DTOs;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    /// <summary>
    /// Represent or implements PolicyRepository.
    /// </summary>
    public class PolicyRepository : IPolicyRepository
    {
        private readonly PolicyDbContext _context;

        public PolicyRepository(PolicyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetByUserIdAsync operation.
        /// </summary>
        public async Task<PagedResult<Policy>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.Policies
                .Include(p => p.Subtype)
                    .ThenInclude(s => s.Type)
                .Where(p => p.UserId == userId)
                .ToPagedResultAsync(page, pageSize);
        }

        /// <summary>
        /// Performs the GetAllAsync operation.
        /// </summary>
        public async Task<PagedResult<Policy>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Policies
                .Include(p => p.Subtype)
                    .ThenInclude(s => s.Type)
                .ToPagedResultAsync(page, pageSize);
        }

        /// <summary>
        /// Performs the GetByIdAsync operation.
        /// </summary>
        public async Task<Policy> GetByIdAsync(Guid policyId)
        {
            return await _context.Policies
                .Include(p => p.Subtype)
                    .ThenInclude(s => s.Type)
                .Include(p => p.PolicyDetail)
                .Include(p => p.HomeDetail)
                .Include(p => p.VehicleDetail)
                .FirstOrDefaultAsync(p => p.PolicyId == policyId);
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(Policy policy)
        {
            await _context.Policies.AddAsync(policy);
        }

        /// <summary>
        /// Performs the UpdateAsync operation.
        /// </summary>
        public async Task UpdateAsync(Policy policy)
        {
            _context.Policies.Update(policy);
        }

        /// <summary>
        /// Performs the CancelAsync operation.
        /// </summary>
        public async Task CancelAsync(Guid policyId)
        {
            var policy = await _context.Policies.FindAsync(policyId);
            if (policy != null)
            {
                policy.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Performs the DeleteAsync operation.
        /// </summary>
        public async Task DeleteAsync(Guid policyId)
        {
            var policy = await _context.Policies.FindAsync(policyId);
            if (policy != null)
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Performs the GetDetailByPolicyIdAsync operation.
        /// </summary>
        public async Task<PolicyDetail> GetDetailByPolicyIdAsync(Guid policyId)
        {
            return await _context.PolicyDetails.FirstOrDefaultAsync(pd => pd.PolicyId == policyId);
        }

        /// <summary>
        /// Performs the AddOrUpdateDetailAsync operation.
        /// </summary>
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

        /// <summary>
        /// Performs the GetHomeDetailByPolicyIdAsync operation.
        /// </summary>
        public async Task<HomeDetail> GetHomeDetailByPolicyIdAsync(Guid policyId)
        {
            return await _context.HomeDetails.FirstOrDefaultAsync(hd => hd.PolicyId == policyId);
        }

        /// <summary>
        /// Performs the AddOrUpdateHomeDetailAsync operation.
        /// </summary>
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

        /// <summary>
        /// Performs the GetVehicleDetailByPolicyIdAsync operation.
        /// </summary>
        public async Task<VehicleDetail> GetVehicleDetailByPolicyIdAsync(Guid policyId)
        {
            return await _context.VehicleDetails.FirstOrDefaultAsync(vd => vd.PolicyId == policyId);
        }

        /// <summary>
        /// Performs the AddOrUpdateVehicleDetailAsync operation.
        /// </summary>
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

        /// <summary>
        /// Performs the AddPaymentAsync operation.
        /// </summary>
        public async Task AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        /// <summary>
        /// Performs the SaveChangesAsync operation.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
