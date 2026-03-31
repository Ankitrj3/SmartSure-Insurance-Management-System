using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories
{
    /// <summary>
    /// Represent or implements IReportRepository.
    /// </summary>
    public interface IReportRepository
    {
        Task<List<Report>> GetAllAsync();
        Task<Report> GetByIdAsync(Guid reportId);
        Task AddAsync(Report report);
        Task DeleteAsync(Report report);
        Task SaveChangesAsync();
    }
}
