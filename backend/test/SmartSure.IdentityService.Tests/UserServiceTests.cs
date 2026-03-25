using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Repositories;
using IdentityService.Services;
using Moq;
using SmartSure.Shared.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.IdentityService.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _userService = new UserService(_repoMock.Object);
        }

        [Fact]
        public async Task AssignRole_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _userService.AssignRole(userId, roleId));
        }

        [Fact]
        public async Task AssignRole_RoleNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var user = new User { UserId = userId, FullName = "Test", Email = "test@test.com" };
            
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _repoMock.Setup(r => r.GetRoleByIdAsync(roleId)).ReturnsAsync((Role)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _userService.AssignRole(userId, roleId));
        }

        [Fact]
        public async Task AssignRole_ValidData_AssignsRoleSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var user = new User { UserId = userId, FullName = "Test", Email = "test@test.com" };
            var role = new Role { RoleId = roleId, RoleName = "Admin" };
            
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _repoMock.Setup(r => r.GetRoleByIdAsync(roleId)).ReturnsAsync(role);

            // Act
            await _userService.AssignRole(userId, roleId);

            // Assert
            _repoMock.Verify(r => r.AddUserRoleAsync(It.IsAny<UserRole>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_UserExists_DeletesUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId, FullName = "Test", Email = "test@test.com" };
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            await _userService.DeleteUser(userId);

            // Assert
            _repoMock.Verify(r => r.Delete(user), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
