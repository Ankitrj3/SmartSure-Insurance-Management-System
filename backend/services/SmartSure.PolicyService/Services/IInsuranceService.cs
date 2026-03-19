using SmartSure.PolicyService.DTOs;

namespace SmartSure.PolicyService.Services
{
    public interface IInsuranceService
    {
        Task<List<InsuranceTypeDTO>> GetAllTypesAsync();
        Task<InsuranceTypeDTO> GetTypeByIdAsync(Guid typeId);
        Task<InsuranceTypeDTO> CreateTypeAsync(CreateInsuranceTypeDTO dto);
        Task UpdateTypeAsync(Guid typeId, UpdateInsuranceTypeDTO dto);
        
        Task<List<InsuranceSubtypeDTO>> GetSubtypesByTypeIdAsync(Guid typeId);
        Task<InsuranceSubtypeDTO> CreateSubtypeAsync(CreateInsuranceSubtypeDTO dto);
        Task UpdateSubtypeAsync(Guid subtypeId, UpdateInsuranceSubtypeDTO dto);
    }
}
