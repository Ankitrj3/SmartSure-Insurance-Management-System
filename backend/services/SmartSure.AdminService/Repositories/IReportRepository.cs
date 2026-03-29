using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories
{
    public interface IReportRepository
    {
        Task<List<Report>> GetAllAsync();
        Task<Report> GetByIdAsync(Guid reportId);
        Task AddAsync(Report report);
        Task DeleteAsync(Report report);
        Task SaveChangesAsync();
    }
}
