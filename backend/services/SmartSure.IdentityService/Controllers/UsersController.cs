using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.Shared.Contracts.Exceptions;

namespace IdentityService.Controllers
{
    /// <summary>
    /// Administrator endpoints for managing application users, role assignments, and deletions.
    /// Restricted entirely to users holding the 'Admin' role.
    /// </summary>
    [Route("auth/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        #region User Administration

        /// <summary>
        /// Retrieves a complete list of all registered users.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetUsers();
                return Ok(users);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Grants or changes a specific role assignment for a given user.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}/roles")]
        public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleDTO dto)
        {
            try
            {
                await _userService.AssignRole(userId, dto.RoleId);
                return Ok(new { message = "Role assigned successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Permanently deletes a user identifier from the authentication database.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try
            {
                await _userService.DeleteUser(userId);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        #endregion
    }
}
