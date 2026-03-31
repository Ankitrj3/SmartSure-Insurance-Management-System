using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories
{
    /// <summary>
    /// Represent or implements ReportRepository.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly AdminDbContext _context;

        public ReportRepository(AdminDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetAllAsync operation.
        /// </summary>
        public async Task<List<Report>> GetAllAsync()
        {
            return await _context.Reports
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the GetByIdAsync operation.
        /// </summary>
        public async Task<Report> GetByIdAsync(Guid reportId)
        {
            return await _context.Reports.FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(Report report)
        {
            await _context.Reports.AddAsync(report);
        }

        /// <summary>
        /// Performs the DeleteAsync operation.
        /// </summary>
        public async Task DeleteAsync(Report report)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
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
