using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;

namespace SmartSure.PolicyService.Services
{
    public class InsuranceService : IInsuranceService
    {
        private readonly IInsuranceRepository _repo;

        public InsuranceService(IInsuranceRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<InsuranceTypeDTO>> GetAllTypesAsync()
        {
            var types = await _repo.GetAllTypesAsync();
            return types.Select(t => new InsuranceTypeDTO
            {
                TypeId = t.TypeId,
                Name = t.Name,
                Description = t.Description
            }).ToList();
        }

        public async Task<InsuranceTypeDTO> GetTypeByIdAsync(Guid typeId)
        {
            var type = await _repo.GetTypeByIdAsync(typeId);
            if (type == null) return null;
            return new InsuranceTypeDTO
            {
                TypeId = type.TypeId,
                Name = type.Name,
                Description = type.Description
            };
        }

        public async Task<InsuranceTypeDTO> CreateTypeAsync(CreateInsuranceTypeDTO dto)
        {
            var type = new InsuranceType
            {
                TypeId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description
            };
            await _repo.AddTypeAsync(type);
            await _repo.SaveChangesAsync();
            return new InsuranceTypeDTO
            {
                TypeId = type.TypeId,
                Name = type.Name,
                Description = type.Description
            };
        }

        public async Task UpdateTypeAsync(Guid typeId, UpdateInsuranceTypeDTO dto)
        {
            var type = await _repo.GetTypeByIdAsync(typeId);
            if (type == null) throw new Exception("Type not found");

            type.Name = dto.Name;
            type.Description = dto.Description;
            
            await _repo.UpdateTypeAsync(type);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteTypeAsync(Guid typeId)
        {
            await _repo.DeleteTypeAsync(typeId);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<InsuranceSubtypeDTO>> GetAllSubtypesAsync()
        {
            var subtypes = await _repo.GetAllSubtypesAsync();
            return subtypes.Select(s => new InsuranceSubtypeDTO
            {
                SubtypeId = s.SubtypeId,
                TypeId = s.TypeId,
                Name = s.Name,
                Description = s.Description,
                BasePremium = s.BasePremium
            }).ToList();
        }

        public async Task<List<InsuranceSubtypeDTO>> GetSubtypesByTypeIdAsync(Guid typeId)
        {
            var subtypes = await _repo.GetSubtypesByTypeIdAsync(typeId);
            return subtypes.Select(s => new InsuranceSubtypeDTO
            {
                SubtypeId = s.SubtypeId,
                TypeId = s.TypeId,
                Name = s.Name,
                Description = s.Description,
                BasePremium = s.BasePremium
            }).ToList();
        }

        public async Task<InsuranceSubtypeDTO> CreateSubtypeAsync(CreateInsuranceSubtypeDTO dto)
        {
            var subtype = new InsuranceSubtype
            {
                SubtypeId = Guid.NewGuid(),
                TypeId = dto.TypeId,
                Name = dto.Name,
                Description = dto.Description,
                BasePremium = dto.BasePremium
            };
            await _repo.AddSubtypeAsync(subtype);
            await _repo.SaveChangesAsync();
            return new InsuranceSubtypeDTO
            {
                SubtypeId = subtype.SubtypeId,
                TypeId = subtype.TypeId,
                Name = subtype.Name,
                Description = subtype.Description,
                BasePremium = subtype.BasePremium
            };
        }

        public async Task UpdateSubtypeAsync(Guid subtypeId, UpdateInsuranceSubtypeDTO dto)
        {
            var subtype = await _repo.GetSubtypeByIdAsync(subtypeId);
            if (subtype == null) throw new Exception("Subtype not found");

            subtype.Name = dto.Name;
            subtype.Description = dto.Description;
            subtype.BasePremium = dto.BasePremium;

            await _repo.UpdateSubtypeAsync(subtype);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteSubtypeAsync(Guid subtypeId)
        {
            await _repo.DeleteSubtypeAsync(subtypeId);
            await _repo.SaveChangesAsync();
        }
    }
}
