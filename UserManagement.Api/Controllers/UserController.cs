using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Domain.DTOs;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register([FromBody] UserRegistrationRequest request)
        {
            try
            {
                var result = await _userService.RegisterUserAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponse>> Update(string id, [FromBody] UserUpdateRequest request)
        {
            try
            {
                var result = await _userService.UpdateUserAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // [Authorize(Roles = "Admin")] // This would be the production attribute
        [HttpPatch("{id}/management")]
        public async Task<ActionResult<UserResponse>> UpdateManagement(
            string id, 
            [FromBody] UserRoleStatusUpdateRequest request,
            [FromHeader(Name = "X-Admin-User-Id")] string performingUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(performingUserId))
                {
                    return BadRequest(new { message = "Performing User ID is required in X-Admin-User-Id header for and authorization." });
                }

                var result = await _userService.UpdateUserRoleStatusAsync(id, request, performingUserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
