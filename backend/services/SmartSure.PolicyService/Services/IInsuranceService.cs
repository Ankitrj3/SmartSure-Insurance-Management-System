using SmartSure.PolicyService.DTOs;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Represent or implements IInsuranceService.
    /// </summary>
    public interface IInsuranceService
    {
        Task<List<InsuranceTypeDTO>> GetAllTypesAsync();
        Task<InsuranceTypeDTO> GetTypeByIdAsync(Guid typeId);
        Task<InsuranceTypeDTO> CreateTypeAsync(CreateInsuranceTypeDTO dto);
        Task UpdateTypeAsync(Guid typeId, UpdateInsuranceTypeDTO dto);
        Task DeleteTypeAsync(Guid typeId);
        
        Task<List<InsuranceSubtypeDTO>> GetAllSubtypesAsync();
        Task<List<InsuranceSubtypeDTO>> GetSubtypesByTypeIdAsync(Guid typeId);
        Task<InsuranceSubtypeDTO> CreateSubtypeAsync(CreateInsuranceSubtypeDTO dto);
        Task UpdateSubtypeAsync(Guid subtypeId, UpdateInsuranceSubtypeDTO dto);
        Task DeleteSubtypeAsync(Guid subtypeId);
    }
}
