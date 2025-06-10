using api.Authorization;
using api.DTOs;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("v1/device")]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceRepository _repo;
        private readonly JwtTokenHelper _jwt;
        public DeviceController(IDeviceRepository r, JwtTokenHelper j) { _repo = r; _jwt = j; }

        [HttpPost("register")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Register(DeviceRegisterDto dto)
        {
            if (await _repo.ExistsAsync(dto.Mac)) return Conflict();
            await _repo.AddAsync(new Device { Mac = dto.Mac, Name = dto.Name });
            await _repo.SaveAsync();
            return Created($"/v1/device/{dto.Mac}", null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(DeviceLoginDto dto)
        {
            if (dto.Password != "worker" || !await _repo.ExistsAsync(dto.Mac))
                return Unauthorized();
            var claims = new[] {
                new Claim(ClaimTypes.Name, dto.Mac),
                new Claim(ClaimTypes.Role, UserRoles.Device)
            };
            return Ok(new { token = _jwt.Generate(claims) });
        }
    }
}
