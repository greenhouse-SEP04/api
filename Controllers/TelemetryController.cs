using api.Authorization;
using api.DTOs;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                Tamper = dto.Tamper,
                Timestamp = DateTime.UtcNow
            };
            await _tele.AddAsync(t);
            await _tele.SaveAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] string dev, int limit = 100)
        {
            if (User.IsInRole(UserRoles.Device) && User.Identity?.Name != dev) return Forbid();
            var data = await _tele.GetLatestAsync(dev, limit);
            return Ok(data);
        }
    }
}
