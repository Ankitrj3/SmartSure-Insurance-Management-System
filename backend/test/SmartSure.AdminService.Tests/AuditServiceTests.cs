using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Models;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.AdminService.Tests
{
    public class AuditServiceTests
    {
        private AdminDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AdminDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AdminDbContext(options);
        }

        [Fact]
        public async Task LogAsync_AddsLogToDatabase()
        {
            // Arrange
            var dbContext = GetDbContext();
            var loggerMock = new Mock<ILogger<AuditService>>();
            var auditService = new AuditService(dbContext, loggerMock.Object);

            var action = "UpdatePolicy";
            var entityType = "Policy";
            var entityId = Guid.NewGuid();
            var actorId = Guid.NewGuid();

            // Act
            await auditService.LogAsync(action, entityType, entityId, actorId, "Details");

            // Assert
            var count = await dbContext.AuditLogs.CountAsync();
            Assert.Equal(1, count);
            var log = await dbContext.AuditLogs.FirstAsync();
            Assert.Equal(action, log.Action);
            Assert.Equal(entityType, log.EntityType);
        }

        [Fact]
        public async Task GetAuditLogsAsync_ReturnsPagedLogs()
        {
            // Arrange
            var dbContext = GetDbContext();
            var loggerMock = new Mock<ILogger<AuditService>>();
            var auditService = new AuditService(dbContext, loggerMock.Object);

            for (int i = 0; i < 5; i++)
            {
                dbContext.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = $"Action {i}",
                    EntityType = "Test",
                    Timestamp = DateTime.UtcNow.AddMinutes(i)
                });
            }
            await dbContext.SaveChangesAsync();

            // Act
            var logs = await auditService.GetAuditLogsAsync(page: 1, pageSize: 2);

            // Assert
            Assert.Equal(2, logs.Count);
        }
    }
}
