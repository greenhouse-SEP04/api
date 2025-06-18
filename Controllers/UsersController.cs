using api.Authorization;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;      // still used for listing / delete
        private readonly UserManager<User> _mgr;

        public UsersController(IUserRepository repo, UserManager<User> mgr)
        {
            _repo = repo;
            _mgr = mgr;
        }

        /* ─────────────────────────────
         *  LIST  (admin only)
         * ──────────────────────────── */
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> List()
            => Ok(await _repo.GetAllAsync());

        /* ─────────────────────────────
         *  CREATE  (admin only)
         * ──────────────────────────── */
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            // 1) username already taken?
            if (await _mgr.FindByNameAsync(dto.Username) is not null)
                return Conflict($"Username '{dto.Username}' is already taken.");

            // 2) create & hash password via UserManager
            var user = new User { UserName = dto.Username };
            var result = await _mgr.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _mgr.AddToRoleAsync(user, UserRoles.User);
            return Created($"/v1/users/{user.Id}", null);
        }

        /* ─────────────────────────────
         *  UPDATE  (admin only)
         * ──────────────────────────── */
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Update(string id, UpdateUserDto dto)
        {
            var user = await _mgr.FindByIdAsync(id);
            if (user is null) return NotFound();

            /* — change username, if requested — */
            if (!string.IsNullOrWhiteSpace(dto.Username) &&
                !dto.Username.Equals(user.UserName, StringComparison.OrdinalIgnoreCase))
            {
                if (await _mgr.FindByNameAsync(dto.Username) is not null)
                    return Conflict($"Username '{dto.Username}' is already taken.");

                user.UserName = dto.Username;
            }

            /* — change password, if requested — */
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var hasher = _mgr.PasswordHasher;
                user.PasswordHash = hasher.HashPassword(user, dto.NewPassword);
            }

            var result = await _mgr.UpdateAsync(user);
            return result.Succeeded ? NoContent() : BadRequest(result.Errors);
        }

        /* ─────────────────────────────
         *  DELETE  (admin only)
         * ──────────────────────────── */
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _mgr.FindByIdAsync(id);
            if (user is null) return NotFound();

            await _mgr.DeleteAsync(user);
            return NoContent();
        }

        /* ─────────────────────────────
         *  SELF-SERVICE PASSWORD RESET
         * ──────────────────────────── */
        public record ResetPwdDto(string CurrentPassword, string NewPassword);

        [HttpPost("me/password")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.User}")]
        public async Task<IActionResult> ResetPassword(ResetPwdDto dto)
        {
            var user = await _mgr.GetUserAsync(User);
            if (user is null) return Unauthorized();

            var result = await _mgr.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            return result.Succeeded ? NoContent() : BadRequest(result.Errors);
        }
    }

    /* ─────────── DTOs ─────────── */
    public record CreateUserDto(string Username, string Password);
    public record UpdateUserDto(string? Username, string? NewPassword);
}
