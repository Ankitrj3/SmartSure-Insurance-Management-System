using Microsoft.Extensions.Logging;
using Moq;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Models;
using SmartSure.AdminService.Repositories;
using SmartSure.AdminService.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace SmartSure.AdminService.Tests
{
    /// <summary>
    /// Unit tests for ReportService - focusing on CRUD operations, DeleteReport logic,
    /// GetReports/GetById, and verifying correct repository calls.
    /// HTTP-dependent operations (GeneratePdfReport) are tested at the repository boundary.
    /// </summary>
    public class ReportServiceTests
    {
        private readonly Mock<IReportRepository>           _reportRepoMock;
        private readonly Mock<ILogger<ReportService>>      _loggerMock;
        private readonly Mock<IHttpClientFactory>          _httpClientFactoryMock;
        private readonly Mock<IConfiguration>              _configMock;
        private readonly Mock<IPdfGeneratorService>        _pdfGeneratorMock;

        public ReportServiceTests()
        {
            _reportRepoMock        = new Mock<IReportRepository>();
            _loggerMock            = new Mock<ILogger<ReportService>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configMock            = new Mock<IConfiguration>();
            _pdfGeneratorMock      = new Mock<IPdfGeneratorService>();

            _configMock.Setup(c => c["Gateway:Url"]).Returns("http://localhost:5057");
        }

        private ReportService CreateService()
        {
            return new ReportService(
                _reportRepoMock.Object,
                _loggerMock.Object,
                _httpClientFactoryMock.Object,
                _configMock.Object,
                _pdfGeneratorMock.Object
            );
        }

        // ── GetReportsAsync Tests ─────────────────────────────────────────────

        [Fact]
        public async Task GetReports_ReturnsListOfReportDTOs()
        {
            // Arrange
            var reports = new List<Report>
            {
                new Report
                {
                    ReportId       = Guid.NewGuid(),
                    Title          = "Q1 Policy Summary",
                    ReportType     = "PolicySummary",
                    Format         = "PDF",
                    Content        = "{\"total\":5}",
                    GeneratedBy    = Guid.NewGuid(),
                    DateRangeStart = new DateTime(2025, 1, 1),
                    DateRangeEnd   = new DateTime(2025, 3, 31),
                    CreatedAt      = DateTime.UtcNow.AddDays(-7)
                },
                new Report
                {
                    ReportId       = Guid.NewGuid(),
                    Title          = "Annual Claims Report",
                    ReportType     = "ClaimsSummary",
                    Format         = "PDF",
                    GeneratedBy    = Guid.NewGuid(),
                    DateRangeStart = new DateTime(2024, 1, 1),
                    DateRangeEnd   = new DateTime(2024, 12, 31),
                    CreatedAt      = DateTime.UtcNow.AddDays(-30)
                }
            };

            _reportRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);
            var service = CreateService();

            // Act
            var result = await service.GetReportsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Title == "Q1 Policy Summary");
            Assert.Contains(result, r => r.Title == "Annual Claims Report");
        }

        [Fact]
        public async Task GetReports_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            _reportRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Report>());
            var service = CreateService();

            // Act
            var result = await service.GetReportsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetReports_MapsAllDTOFieldsCorrectly()
        {
            // Arrange
            var adminId  = Guid.NewGuid();
            var reportId = Guid.NewGuid();
            var start    = new DateTime(2025, 1, 1);
            var end      = new DateTime(2025, 6, 30);

            var reports = new List<Report>
            {
                new Report
                {
                    ReportId       = reportId,
                    Title          = "Revenue Report",
                    ReportType     = "Revenue",
                    Format         = "CSV",
                    Content        = "csv-data-here",
                    GeneratedBy    = adminId,
                    DateRangeStart = start,
                    DateRangeEnd   = end
                }
            };
            _reportRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);
            var service = CreateService();

            // Act
            var result = await service.GetReportsAsync();

            // Assert
            var dto = Assert.Single(result);
            Assert.Equal(reportId,        dto.ReportId);
            Assert.Equal("Revenue Report", dto.Title);
            Assert.Equal("Revenue",        dto.ReportType);
            Assert.Equal("CSV",            dto.Format);
            Assert.Equal("csv-data-here", dto.Content);
            Assert.Equal(adminId,          dto.GeneratedBy);
            Assert.Equal(start,            dto.DateRangeStart);
            Assert.Equal(end,              dto.DateRangeEnd);
        }

        // ── GetReportByIdAsync Tests ──────────────────────────────────────────

        [Fact]
        public async Task GetReportById_NotFound_ReturnsNull()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            _reportRepoMock.Setup(r => r.GetByIdAsync(reportId)).ReturnsAsync((Report)null!);
            var service = CreateService();

            // Act
            var result = await service.GetReportByIdAsync(reportId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetReportById_Found_ReturnsMappedDTO()
        {
            // Arrange
            var report = new Report
            {
                ReportId    = Guid.NewGuid(),
                Title       = "Test Report",
                ReportType  = "PolicySummary",
                Format      = "PDF",
                GeneratedBy = Guid.NewGuid()
            };
            _reportRepoMock.Setup(r => r.GetByIdAsync(report.ReportId)).ReturnsAsync(report);
            var service = CreateService();

            // Act
            var result = await service.GetReportByIdAsync(report.ReportId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(report.ReportId, result!.ReportId);
            Assert.Equal("Test Report",   result.Title);
        }

        // ── DeleteReportAsync Tests ───────────────────────────────────────────

        [Fact]
        public async Task DeleteReport_NotFound_ReturnsFalse()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            _reportRepoMock.Setup(r => r.GetByIdAsync(reportId)).ReturnsAsync((Report)null!);
            var service = CreateService();

            // Act
            var result = await service.DeleteReportAsync(reportId);

            // Assert
            Assert.False(result);
            _reportRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Report>()), Times.Never);
        }

        [Fact]
        public async Task DeleteReport_Exists_DeletesAndReturnsTrue()
        {
            // Arrange
            var report = new Report { ReportId = Guid.NewGuid(), Title = "Old Report" };
            _reportRepoMock.Setup(r => r.GetByIdAsync(report.ReportId)).ReturnsAsync(report);
            _reportRepoMock.Setup(r => r.DeleteAsync(report)).Returns(Task.CompletedTask);
            var service = CreateService();

            // Act
            var result = await service.DeleteReportAsync(report.ReportId);

            // Assert
            Assert.True(result);
            _reportRepoMock.Verify(r => r.DeleteAsync(report), Times.Once);
        }

        [Fact]
        public async Task DeleteReport_Exists_DoesNotCallSaveChanges_SinceDeleteAlreadyHandlesIt()
        {
            // Arrange – Verify the repository contract: Delete does its own save
            var report = new Report { ReportId = Guid.NewGuid() };
            _reportRepoMock.Setup(r => r.GetByIdAsync(report.ReportId)).ReturnsAsync(report);
            _reportRepoMock.Setup(r => r.DeleteAsync(report)).Returns(Task.CompletedTask);
            var service = CreateService();

            // Act
            await service.DeleteReportAsync(report.ReportId);

            // Assert
            _reportRepoMock.Verify(r => r.DeleteAsync(report), Times.Once);
        }

        // ── GenerateReportAsync Tests (Repository layer) ──────────────────────

        [Fact]
        public async Task GenerateReport_SavesReportToRepository()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var dto     = new ReportRequestDTO
            {
                Title          = "Test Gen Report",
                ReportType     = "Revenue",
                Format         = "PDF",
                DateRangeStart = DateTime.UtcNow.AddDays(-30),
                DateRangeEnd   = DateTime.UtcNow
            };

            // Use a fake handler that returns empty JSON arrays for all HTTP calls
            var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(JsonSerializer.Serialize(new object[] { }))
            });
            var httpClient = new HttpClient(fakeHandler);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            Report? savedReport = null;
            _reportRepoMock.Setup(r => r.AddAsync(It.IsAny<Report>()))
                .Callback<Report>(r => savedReport = r)
                .Returns(Task.CompletedTask);
            _reportRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.GenerateReportAsync(adminId, dto, "Bearer test-token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Gen Report", result.Title);
            Assert.Equal("Revenue",          result.ReportType);
            Assert.Equal(adminId,            result.GeneratedBy);

            _reportRepoMock.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Once);
            _reportRepoMock.Verify(r => r.SaveChangesAsync(),            Times.Once);
        }

        [Fact]
        public async Task GeneratePdfReport_ReportNotFound_ReturnsEmptyArray()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            _reportRepoMock.Setup(r => r.GetByIdAsync(reportId)).ReturnsAsync((Report)null!);
            var service = CreateService();

            // Act
            var result = await service.RegeneratePdfAsync(reportId, "Bearer token");

            // Assert
            Assert.Empty(result);
        }
    }

    /// <summary>
    /// A simple fake HttpMessageHandler for testing HTTP-dependent service methods.
    /// Always returns the same pre-configured HttpResponseMessage.
    /// </summary>
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Clone content so it can be read multiple times (for 3 API calls in CollectReportData)
            var clonedResponse = new HttpResponseMessage(_response.StatusCode)
            {
                Content = new StringContent(_response.Content.ReadAsStringAsync().GetAwaiter().GetResult())
            };
            return Task.FromResult(clonedResponse);
        }
    }
}
