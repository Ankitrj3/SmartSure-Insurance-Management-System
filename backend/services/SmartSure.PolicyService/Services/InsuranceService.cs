using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Service for managing insurance products, including types and subtypes.
    /// Encapsulates the core business logic and repository interactions for insurance configurations.
    /// </summary>
    public class InsuranceService : IInsuranceService
    {
        private readonly IInsuranceRepository _repo;

        public InsuranceService(IInsuranceRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Retrieves all top-level insurance types available in the system.
        /// </summary>
        /// <returns>A list of data transfer objects representing the insurance types.</returns>
        public async Task<List<InsuranceTypeDTO>> GetAllTypesAsync()
        {
            var types = await _repo.GetAllTypesAsync();
            return types.Select(t => new InsuranceTypeDTO
            {
                TypeId      = t.TypeId,
                Name        = t.Name,
                Description = t.Description
            }).ToList();
        }

        /// <summary>
        /// Retrieves a specific insurance type by its unique identifier.
        /// </summary>
        /// <param name="typeId">The unique guide identifying the insurance type.</param>
        /// <returns>The specified insurance type DTO, or null if not found.</returns>
        public async Task<InsuranceTypeDTO> GetTypeByIdAsync(Guid typeId)
        {
            var type = await _repo.GetTypeByIdAsync(typeId);
            if (type == null) return null!;
            return new InsuranceTypeDTO
            {
                TypeId      = type.TypeId,
                Name        = type.Name,
                Description = type.Description
            };
        }

        /// <summary>
        /// Provisions a new top-level insurance type within the system and persists it to the database.
        /// </summary>
        /// <param name="dto">The model containing the parameters for the new insurance type.</param>
        /// <returns>The newly created insurance type as a DTO.</returns>
        public async Task<InsuranceTypeDTO> CreateTypeAsync(CreateInsuranceTypeDTO dto)
        {
            var type = new InsuranceType
            {
                TypeId      = Guid.NewGuid(),
                Name        = dto.Name,
                Description = dto.Description
            };
            await _repo.AddTypeAsync(type);
            await _repo.SaveChangesAsync();
            return new InsuranceTypeDTO
            {
                TypeId      = type.TypeId,
                Name        = type.Name,
                Description = type.Description
            };
        }

        /// <summary>
        /// Updates the metadata (name and description) of an existing insurance type.
        /// </summary>
        /// <param name="typeId">The identifier of the insurance type to modify.</param>
        /// <param name="dto">The updated details.</param>
        /// <exception cref="NotFoundException">Thrown when the specified insurance type does not exist.</exception>
        public async Task UpdateTypeAsync(Guid typeId, UpdateInsuranceTypeDTO dto)
        {
            var type = await _repo.GetTypeByIdAsync(typeId);
            if (type == null) throw new NotFoundException("Insurance type", typeId);

            type.Name        = dto.Name;
            type.Description = dto.Description;

            await _repo.UpdateTypeAsync(type);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Hard-deletes an insurance type from the system.
        /// Warning: Cascading deletes may apply to its subtypes based on DB configuration.
        /// </summary>
        /// <param name="typeId">The identifier of the insurance type to remove.</param>
        public async Task DeleteTypeAsync(Guid typeId)
        {
            await _repo.DeleteTypeAsync(typeId);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all insurance subtypes currently configured, including their parent type names.
        /// </summary>
        /// <returns>A flat collection of all insurance subtypes.</returns>
        public async Task<List<InsuranceSubtypeDTO>> GetAllSubtypesAsync()
        {
            var subtypes = await _repo.GetAllSubtypesAsync();
            return subtypes.Select(s => new InsuranceSubtypeDTO
            {
                SubtypeId   = s.SubtypeId,
                TypeId      = s.TypeId,
                TypeName    = s.Type?.Name,
                Name        = s.Name,
                Description = s.Description,
                BasePremium = s.BasePremium
            }).ToList();
        }

        /// <summary>
        /// Retrieves all insurance subtypes associated with a specific top-level insurance type.
        /// </summary>
        /// <param name="typeId">The parent insurance type identifier.</param>
        /// <returns>A collection of matching insurance subtypes.</returns>
        public async Task<List<InsuranceSubtypeDTO>> GetSubtypesByTypeIdAsync(Guid typeId)
        {
            var subtypes = await _repo.GetSubtypesByTypeIdAsync(typeId);
            return subtypes.Select(s => new InsuranceSubtypeDTO
            {
                SubtypeId   = s.SubtypeId,
                TypeId      = s.TypeId,
                TypeName    = s.Type?.Name,
                Name        = s.Name,
                Description = s.Description,
                BasePremium = s.BasePremium
            }).ToList();
        }

        /// <summary>
        /// Creates a new insurance subtype mapped under an existing parent insurance type.
        /// Includes standard details and the required base premium amount.
        /// </summary>
        /// <param name="dto">The creation payload with required parameters.</param>
        /// <returns>The newly created subtype DTO enriched with its parent type name.</returns>
        public async Task<InsuranceSubtypeDTO> CreateSubtypeAsync(CreateInsuranceSubtypeDTO dto)
        {
            var subtype = new InsuranceSubtype
            {
                SubtypeId   = Guid.NewGuid(),
                TypeId      = dto.TypeId,
                Name        = dto.Name,
                Description = dto.Description,
                BasePremium = dto.BasePremium
            };
            await _repo.AddSubtypeAsync(subtype);
            await _repo.SaveChangesAsync();

            var parentType = await _repo.GetTypeByIdAsync(dto.TypeId);

            return new InsuranceSubtypeDTO
            {
                SubtypeId   = subtype.SubtypeId,
                TypeId      = subtype.TypeId,
                TypeName    = parentType?.Name,
                Name        = subtype.Name,
                Description = subtype.Description,
                BasePremium = subtype.BasePremium
            };
        }

        /// <summary>
        /// Updates details and the financial base premium for a specific configured subtype.
        /// </summary>
        /// <param name="subtypeId">The identifier of the subtype to override.</param>
        /// <param name="dto">The updated model data.</param>
        /// <exception cref="NotFoundException">Thrown if the subtype isn't found.</exception>
        public async Task UpdateSubtypeAsync(Guid subtypeId, UpdateInsuranceSubtypeDTO dto)
        {
            var subtype = await _repo.GetSubtypeByIdAsync(subtypeId);
            if (subtype == null) throw new NotFoundException("Insurance subtype", subtypeId);

            subtype.Name        = dto.Name;
            subtype.Description = dto.Description;
            subtype.BasePremium = dto.BasePremium;

            await _repo.UpdateSubtypeAsync(subtype);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an insurance subtype explicitly. 
        /// </summary>
        /// <param name="subtypeId">The guid denoting the subtype to drop.</param>
        public async Task DeleteSubtypeAsync(Guid subtypeId)
        {
            await _repo.DeleteSubtypeAsync(subtypeId);
            await _repo.SaveChangesAsync();
        }
    }
}
