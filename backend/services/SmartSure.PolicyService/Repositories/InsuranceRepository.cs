using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    public class InsuranceRepository : IInsuranceRepository
    {
        private readonly PolicyDbContext _context;

        public InsuranceRepository(PolicyDbContext context)
        {
            _context = context;
        }

        public async Task<List<InsuranceType>> GetAllTypesAsync()
        {
            return await _context.InsuranceTypes.ToListAsync();
        }

        public async Task<InsuranceType> GetTypeByIdAsync(Guid typeId)
        {
            return await _context.InsuranceTypes.FindAsync(typeId);
        }

        public async Task<List<InsuranceSubtype>> GetSubtypesByTypeIdAsync(Guid typeId)
        {
            return await _context.InsuranceSubtypes.Where(s => s.TypeId == typeId).ToListAsync();
        }

        public async Task<InsuranceSubtype> GetSubtypeByIdAsync(Guid subtypeId)
        {
            return await _context.InsuranceSubtypes.FindAsync(subtypeId);
        }

        public async Task AddTypeAsync(InsuranceType type)
        {
            await _context.InsuranceTypes.AddAsync(type);
        }

        public async Task UpdateTypeAsync(InsuranceType type)
        {
            _context.InsuranceTypes.Update(type);
        }

        public async Task AddSubtypeAsync(InsuranceSubtype subtype)
        {
            await _context.InsuranceSubtypes.AddAsync(subtype);
        }

        public async Task UpdateSubtypeAsync(InsuranceSubtype subtype)
        {
            _context.InsuranceSubtypes.Update(subtype);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
