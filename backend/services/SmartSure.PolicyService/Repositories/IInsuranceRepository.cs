using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    /// <summary>
    /// Represent or implements IInsuranceRepository.
    /// </summary>
    public interface IInsuranceRepository
    {
        Task<List<InsuranceType>> GetAllTypesAsync();
        Task<InsuranceType> GetTypeByIdAsync(Guid typeId);
        Task<List<InsuranceSubtype>> GetAllSubtypesAsync();
        Task<List<InsuranceSubtype>> GetSubtypesByTypeIdAsync(Guid typeId);
        Task<InsuranceSubtype> GetSubtypeByIdAsync(Guid subtypeId);
        Task AddTypeAsync(InsuranceType type);
        Task UpdateTypeAsync(InsuranceType type);
        Task DeleteTypeAsync(Guid typeId);
        Task AddSubtypeAsync(InsuranceSubtype subtype);
        Task UpdateSubtypeAsync(InsuranceSubtype subtype);
        Task DeleteSubtypeAsync(Guid subtypeId);
        Task SaveChangesAsync();
    }
}
