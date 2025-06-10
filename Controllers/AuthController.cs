using api.DTOs;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly JwtTokenHelper _jwt;
        public AuthController(UserManager<ApplicationUser> u, JwtTokenHelper j) { _users = u; _jwt = j; }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _users.FindByNameAsync(dto.Username);
            if (user == null || !await _users.CheckPasswordAsync(user, dto.Password)) return Unauthorized();
            var roles = await _users.GetRolesAsync(user);
            var claims = new List<Claim> {
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.NameIdentifier, user.Id)
            }.Concat(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            return Ok(new { token = _jwt.Generate(claims) });
        }
    }
}
