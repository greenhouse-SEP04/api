using api.Authorization;
using api.DTOs;
using api.Helpers;
using api.Infrastructure;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _users;
        private readonly JwtTokenHelper _jwt;
        private readonly IDeviceRepository _devices;      // if you still need it
        private readonly WorkerCredentials _workerCreds;  // ← NEW

        public AuthController(UserManager<User> users,
                              JwtTokenHelper jwt,
                              IDeviceRepository devices,
                              IOptions<WorkerCredentials> cfg)   // ← NEW
        {
            _users = users;
            _jwt = jwt;
            _devices = devices;
            _workerCreds = cfg.Value;     // store once
        }

        /* ───────────── Unified REGISTER ──────────────────────────────── */
        /* call with ?role=admin|user|device|worker                       */
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromQuery] string? role, Credentials dto)
        {
            var r = role?.Trim().ToLowerInvariant();

            if (r is not ("admin" or "user" or "device" or "worker"))
                return BadRequest("Query string ?role must be admin, user, device, or worker.");

            /* 1️⃣  Username already taken? */
            if (await _users.FindByNameAsync(dto.Username) is not null)
                return Conflict($"{dto.Username} already registered.");

            /* 2️⃣  Create Identity user */
            var newUser = new User { UserName = dto.Username };
            var result = await _users.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            /* 3️⃣  Attach role-specific data */
            switch (r)
            {
                case "admin":
                    await _users.AddToRoleAsync(newUser, UserRoles.Admin);
                    break;

                case "user":
                    await _users.AddToRoleAsync(newUser, UserRoles.User);
                    break;

                case "device":
                    await _users.AddToRoleAsync(newUser, UserRoles.Device);

                    // ensure Device row exists
                    if (!await _devices.ExistsAsync(dto.Username))
                    {
                        await _devices.AddAsync(new Device { Mac = dto.Username });
                        await _devices.SaveAsync();
                    }
                    break;

                case "worker":
                    await _users.AddToRoleAsync(newUser, UserRoles.Worker);
                    break;
            }

            /* 4️⃣  Build token */
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, newUser.UserName!),
        new Claim(ClaimTypes.Role, r switch
        {
            "admin"  => UserRoles.Admin,
            "user"   => UserRoles.User,
            "device" => UserRoles.Device,
            _        => UserRoles.Worker
        })
    };

            var token = _jwt.Generate(claims);
            return Created("/v1/auth/login", new { token });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(Credentials dto)
        {
            IEnumerable<Claim> claims;

            /* 1️ Worker secret? */
            if (string.Equals(dto.Username, _workerCreds.Username,
                              StringComparison.OrdinalIgnoreCase) &&
                dto.Password == _workerCreds.Password)
            {
                claims = new[]
                {
            new Claim(ClaimTypes.Name, _workerCreds.Username),
            new Claim(ClaimTypes.Role, UserRoles.Worker)
        };
            }
            else
            {
                /* 2️ Identity users */
                var user = await _users.FindByNameAsync(dto.Username);
                if (user is null || !await _users.CheckPasswordAsync(user, dto.Password))
                    return Unauthorized();

                var roles = await _users.GetRolesAsync(user);
                claims = new List<Claim>
        {
            new(ClaimTypes.Name,           user.UserName!),
            new(ClaimTypes.NameIdentifier, user.Id)
        }.Concat(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            return Ok(new { token = _jwt.Generate(claims) });
        }



    }
}
