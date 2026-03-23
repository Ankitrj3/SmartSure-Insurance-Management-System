using SmartSure.AdminService.DTOs;

namespace SmartSure.AdminService.Services
{
    public interface IReportService
    {
        Task<ReportResponseDTO> GenerateReportAsync(Guid adminId, ReportRequestDTO dto, string token);
        Task<List<ReportResponseDTO>> GetReportsAsync();
        Task<ReportResponseDTO?> GetReportByIdAsync(Guid reportId);
    }
}
