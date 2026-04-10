using Microsoft.Extensions.Logging;
using Moq;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Models;
using SmartSure.AdminService.Repositories;
using SmartSure.AdminService.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.AdminService.Tests
{
    /// <summary>
    /// Unit tests for AuditService - covering log creation, paged retrieval, and count operations.
    /// </summary>
    public class AuditServiceFullTests
    {
        private readonly Mock<IAuditLogRepository>         _repoMock;
        private readonly Mock<ILogger<AuditService>>       _loggerMock;
        private readonly AuditService                      _service;

        public AuditServiceFullTests()
        {
            _repoMock   = new Mock<IAuditLogRepository>();
            _loggerMock = new Mock<ILogger<AuditService>>();
            _service    = new AuditService(_repoMock.Object, _loggerMock.Object);
        }

        // ── LogAsync Tests ────────────────────────────────────────────────────

        [Fact]
        public async Task LogAsync_CreatesAuditLogWithCorrectFields()
        {
            // Arrange
            AuditLog? savedLog  = null;
            var entityId  = Guid.NewGuid();
            var actorId   = Guid.NewGuid();

            _repoMock.Setup(r => r.AddAsync(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(l => savedLog = l)
                .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.LogAsync("UpdatePolicy", "Policy", entityId, actorId, "Changed status to Cancelled");

            // Assert
            _repoMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(),              Times.Once);

            Assert.NotNull(savedLog);
            Assert.Equal("UpdatePolicy",                    savedLog!.Action);
            Assert.Equal("Policy",                         savedLog.EntityType);
            Assert.Equal(entityId,                         savedLog.EntityId);
            Assert.Equal(actorId,                          savedLog.ActorId);
            Assert.Equal("Changed status to Cancelled",    savedLog.Details);
            Assert.NotEqual(Guid.Empty,                    savedLog.Id);
        }

        [Fact]
        public async Task LogAsync_NullDetails_StillCreatesLog()
        {
            // Arrange
            _repoMock.Setup(r => r.AddAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.LogAsync("ViewDashboard", "Dashboard", null, null);

            // Assert
            _repoMock.Verify(r => r.AddAsync(It.Is<AuditLog>(l =>
                l.Action     == "ViewDashboard" &&
                l.EntityType == "Dashboard" &&
                l.EntityId   == null &&
                l.ActorId    == null &&
                l.Details    == null)), Times.Once);
        }

        [Fact]
        public async Task LogAsync_GeneratesUniqueIdForEachLog()
        {
            // Arrange
            var idsSeen = new List<Guid>();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(l => idsSeen.Add(l.Id))
                .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act – log twice
            await _service.LogAsync("Action1", "Type", null, null);
            await _service.LogAsync("Action2", "Type", null, null);

            // Assert
            Assert.Equal(2, idsSeen.Count);
            Assert.NotEqual(idsSeen[0], idsSeen[1]);
        }

        // ── GetAuditLogsAsync Tests ───────────────────────────────────────────

        [Fact]
        public async Task GetAuditLogsAsync_ReturnsMappedDTOs()
        {
            // Arrange
            var logs = new List<AuditLog>
            {
                new AuditLog { Id = Guid.NewGuid(), Action = "Login",        EntityType = "User",   Timestamp = DateTime.UtcNow },
                new AuditLog { Id = Guid.NewGuid(), Action = "UpdatePolicy", EntityType = "Policy", Timestamp = DateTime.UtcNow }
            };
            _repoMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(logs);

            // Act
            var result = await _service.GetAuditLogsAsync(1, 10);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, dto => dto.Action == "Login");
            Assert.Contains(result, dto => dto.Action == "UpdatePolicy");
        }

        [Fact]
        public async Task GetAuditLogsAsync_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _repoMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(new List<AuditLog>());

            // Act
            var result = await _service.GetAuditLogsAsync(1, 10);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAuditLogsAsync_MapsAllDTOFieldsCorrectly()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var actorId  = Guid.NewGuid();
            var ts       = DateTime.UtcNow.AddMinutes(-5);
            var logId    = Guid.NewGuid();

            var logs = new List<AuditLog>
            {
                new AuditLog
                {
                    Id         = logId,
                    Action     = "DeleteUser",
                    EntityType = "User",
                    EntityId   = entityId,
                    ActorId    = actorId,
                    Details    = "Admin deleted user",
                    Timestamp  = ts
                }
            };
            _repoMock.Setup(r => r.GetPagedAsync(1, 5)).ReturnsAsync(logs);

            // Act
            var result = await _service.GetAuditLogsAsync(1, 5);

            // Assert
            var dto = Assert.Single(result);
            Assert.Equal(logId,             dto.Id);
            Assert.Equal("DeleteUser",      dto.Action);
            Assert.Equal("User",            dto.EntityType);
            Assert.Equal(entityId,          dto.EntityId);
            Assert.Equal(actorId,           dto.ActorId);
            Assert.Equal("Admin deleted user", dto.Details);
            Assert.Equal(ts,                dto.Timestamp);
        }

        // ── GetTotalAuditLogsCountAsync Tests ─────────────────────────────────

        [Fact]
        public async Task GetTotalAuditLogsCount_ReturnsCountFromRepository()
        {
            // Arrange
            _repoMock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(42);

            // Act
            var result = await _service.GetTotalAuditLogsCountAsync();

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task GetTotalAuditLogsCount_Empty_ReturnsZero()
        {
            // Arrange
            _repoMock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.GetTotalAuditLogsCountAsync();

            // Assert
            Assert.Equal(0, result);
        }

        // ── Pagination Tests ──────────────────────────────────────────────────

        [Fact]
        public async Task GetAuditLogsAsync_PassesPageParametersToRepository()
        {
            // Arrange
            _repoMock.Setup(r => r.GetPagedAsync(3, 25)).ReturnsAsync(new List<AuditLog>());

            // Act
            await _service.GetAuditLogsAsync(3, 25);

            // Assert
            _repoMock.Verify(r => r.GetPagedAsync(3, 25), Times.Once);
        }
    }
}
