using SmartSure.AdminService.DTOs;

namespace SmartSure.AdminService.Services
{
    /// <summary>
    /// Represent or implements IReportService.
    /// </summary>
    public interface IReportService
    {
        Task<ReportResponseDTO> GenerateReportAsync(Guid adminId, ReportRequestDTO dto, string token);
        Task<List<ReportResponseDTO>> GetReportsAsync();
        Task<ReportResponseDTO?> GetReportByIdAsync(Guid reportId);
        Task<byte[]> GeneratePdfReportAsync(Guid adminId, ReportRequestDTO dto, string token);
        Task<byte[]> RegeneratePdfAsync(Guid reportId, string token);
        Task<bool> DeleteReportAsync(Guid reportId);
    }
}
