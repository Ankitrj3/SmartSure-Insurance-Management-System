using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    public interface IInsuranceRepository
    {
        Task<List<InsuranceType>> GetAllTypesAsync();
        Task<InsuranceType> GetTypeByIdAsync(Guid typeId);
        Task<List<InsuranceSubtype>> GetSubtypesByTypeIdAsync(Guid typeId);
        Task<InsuranceSubtype> GetSubtypeByIdAsync(Guid subtypeId);
        Task AddTypeAsync(InsuranceType type);
        Task UpdateTypeAsync(InsuranceType type);
        Task AddSubtypeAsync(InsuranceSubtype subtype);
        Task UpdateSubtypeAsync(InsuranceSubtype subtype);
        Task SaveChangesAsync();
    }
}
