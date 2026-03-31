using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    /// <summary>
    /// Represent or implements InsuranceRepository.
    /// </summary>
    public class InsuranceRepository : IInsuranceRepository
    {
        private readonly PolicyDbContext _context;

        public InsuranceRepository(PolicyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetAllTypesAsync operation.
        /// </summary>
        public async Task<List<InsuranceType>> GetAllTypesAsync()
        {
            return await _context.InsuranceTypes
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Performs the GetTypeByIdAsync operation.
        /// </summary>
        public async Task<InsuranceType> GetTypeByIdAsync(Guid typeId)
        {
            return await _context.InsuranceTypes.FindAsync(typeId);
        }

        /// <summary>
        /// Performs the GetAllSubtypesAsync operation.
        /// </summary>
        public async Task<List<InsuranceSubtype>> GetAllSubtypesAsync()
        {
            return await _context.InsuranceSubtypes
                .AsNoTracking()
                .Include(s => s.Type)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the GetSubtypesByTypeIdAsync operation.
        /// </summary>
        public async Task<List<InsuranceSubtype>> GetSubtypesByTypeIdAsync(Guid typeId)
        {
            return await _context.InsuranceSubtypes
                .AsNoTracking()
                .Include(s => s.Type)
                .Where(s => s.TypeId == typeId)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the GetSubtypeByIdAsync operation.
        /// </summary>
        public async Task<InsuranceSubtype> GetSubtypeByIdAsync(Guid subtypeId)
        {
            return await _context.InsuranceSubtypes.FindAsync(subtypeId);
        }

        /// <summary>
        /// Performs the AddTypeAsync operation.
        /// </summary>
        public async Task AddTypeAsync(InsuranceType type)
        {
            await _context.InsuranceTypes.AddAsync(type);
        }

        /// <summary>
        /// Performs the UpdateTypeAsync operation.
        /// </summary>
        public async Task UpdateTypeAsync(InsuranceType type)
        {
            _context.InsuranceTypes.Update(type);
        }

        /// <summary>
        /// Performs the DeleteTypeAsync operation.
        /// </summary>
        public async Task DeleteTypeAsync(Guid typeId)
        {
            var type = await _context.InsuranceTypes.FindAsync(typeId);
            if (type != null) _context.InsuranceTypes.Remove(type);
        }

        /// <summary>
        /// Performs the AddSubtypeAsync operation.
        /// </summary>
        public async Task AddSubtypeAsync(InsuranceSubtype subtype)
        {
            await _context.InsuranceSubtypes.AddAsync(subtype);
        }

        /// <summary>
        /// Performs the UpdateSubtypeAsync operation.
        /// </summary>
        public async Task UpdateSubtypeAsync(InsuranceSubtype subtype)
        {
            _context.InsuranceSubtypes.Update(subtype);
        }

        /// <summary>
        /// Performs the DeleteSubtypeAsync operation.
        /// </summary>
        public async Task DeleteSubtypeAsync(Guid subtypeId)
        {
            var subtype = await _context.InsuranceSubtypes.FindAsync(subtypeId);
            if (subtype != null) _context.InsuranceSubtypes.Remove(subtype);
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
