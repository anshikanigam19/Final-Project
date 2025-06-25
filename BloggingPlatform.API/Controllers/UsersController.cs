using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            var user = await _userService.GetUserByIdAsync(int.Parse(userId));

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<AuthResponseDTO>> ChangePassword(ChangePasswordDTO changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            var result = await _userService.ChangePasswordAsync(int.Parse(userId), changePasswordDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<UserDTO>> UpdateProfile(UpdateProfileDTO updateProfileDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            var updatedUser = await _userService.UpdateProfileAsync(int.Parse(userId), updateProfileDto);

            if (updatedUser == null)
                return NotFound(new { message = "User not found." });

            return Ok(updatedUser);
        }
    }
}