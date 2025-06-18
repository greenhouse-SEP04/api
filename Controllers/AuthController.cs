using api.Authorization;
using api.DTOs;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;      // ← NEW
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _users;
        private readonly JwtTokenHelper _jwt;
        private readonly IDeviceRepository _devices;   // ← NEW

        public AuthController(UserManager<User> u,
                              JwtTokenHelper j,
                              IDeviceRepository d)          // ← NEW
        {
            _users = u;
            _jwt = j;
            _devices = d;
        }

        /* ───────────── DEVICE SELF-REGISTER ───────────────────────── */
        [HttpPost("register/device")]
        public async Task<IActionResult> RegisterDevice(DeviceSelfRegisterDto dto)
        {
            // 1) username (MAC) already taken?
            if (await _users.FindByNameAsync(dto.Username) is not null)
                return Conflict($"MAC {dto.Username} already registered.");

            // 2) create identity as Device
            var user = new User { UserName = dto.Username };
            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _users.AddToRoleAsync(user, UserRoles.Device);

            // 3) ensure Device entity exists
            if (!await _devices.ExistsAsync(dto.Username))
            {
                await _devices.AddAsync(new Device { Mac = dto.Username });
                await _devices.SaveAsync();
            }

            // 4) build token
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,  user.UserName!),
                new Claim(ClaimTypes.Role,  UserRoles.Device)
            };

            var token = _jwt.Generate(claims);
            return Created("/v1/auth/login", new { token });
        }

        /* ───────────── GENERIC LOGIN  ────────────────────────────────── */
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _users.FindByNameAsync(dto.Username);
            if (user is null ||
                !await _users.CheckPasswordAsync(user, dto.Password))
                return Unauthorized();

            var roles = await _users.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name,           user.UserName!),
                new(ClaimTypes.NameIdentifier, user.Id)
            }.Concat(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            return Ok(new { token = _jwt.Generate(claims) });
        }
    }
    public record DeviceSelfRegisterDto(string Username /* MAC */,
                                    string Password);

}
