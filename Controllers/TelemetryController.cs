using api.Authorization;
using api.DTOs;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("v1/telemetry")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryRepository _tele;
        private readonly IDeviceRepository _dev;
        public TelemetryController(ITelemetryRepository t, IDeviceRepository d) { _tele = t; _dev = d; }

        [HttpPost]
        [Authorize(Roles = UserRoles.Device + "," + UserRoles.Admin)]
        public async Task<IActionResult> Post([FromQuery] string dev, TelemetryDto dto)
        {
            if (User.IsInRole(UserRoles.Device) && User.Identity?.Name != dev) return Forbid();
            if (!await _dev.ExistsAsync(dev)) return NotFound();

            var t = new Telemetry
            {
                DeviceMac = dev,
                Temperature = dto.Temperature,
                Humidity = dto.Humidity,
                Soil = dto.Soil,
                Lux = dto.Lux,
                Level = dto.Level,
                Motion = dto.Motion,
                Tamper = dto.Tamper,
                AccelX = dto.Accel[0],
                AccelY = dto.Accel[1],
                AccelZ = dto.Accel[2],
                CfgRev = dto.CfgRev,
                Timestamp = DateTime.TryParse(dto.Ts, out var ts) ? ts : DateTime.UtcNow
            };
            await _tele.AddAsync(t);
            await _tele.SaveAsync();
            return Ok();
        }

        // ─────────────────────────────────────────────────────────────
        // 5. TELEMETRY  – admins, owners, and the Worker token
        [HttpGet("{mac}/telemetry")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.User},{UserRoles.Worker}")]
        public async Task<IActionResult> Telemetry(string mac, int limit = 100)
        {
            var dev = await _dev.GetAsync(mac);
            if (dev == null) return NotFound();

            bool isAdmin = User.IsInRole(UserRoles.Admin);
            bool isOwner = dev.OwnerId != null &&
                            User.FindFirstValue(ClaimTypes.NameIdentifier) == dev.OwnerId;
            bool isWorker = User.IsInRole(UserRoles.Worker);

            if (!(isAdmin || isOwner || isWorker))
                return Forbid();

            var data = await _tele.GetLatestAsync(mac, limit);
            return Ok(data);
        }
    }
}
