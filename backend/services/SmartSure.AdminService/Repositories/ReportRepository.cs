using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AdminDbContext _context;

        public ReportRepository(AdminDbContext context)
        {
            _context = context;
        }

        public async Task<List<Report>> GetAllAsync()
        {
            return await _context.Reports
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Report> GetByIdAsync(Guid reportId)
        {
            return await _context.Reports.FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        public async Task AddAsync(Report report)
        {
            await _context.Reports.AddAsync(report);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
