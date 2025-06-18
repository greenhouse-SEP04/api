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
    [Route("v1/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsRepository _settings;
        private readonly IDeviceRepository _devices;
        public SettingsController(ISettingsRepository s, IDeviceRepository d) { _settings = s; _devices = d; }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.User},{UserRoles.Device},{UserRoles.Worker}")]
        public async Task<IActionResult> Get([FromQuery] string? dev)
        {
            bool isDevice =
                User.IsInRole(UserRoles.Device);
            bool isWorker = User.IsInRole(UserRoles.Worker);

            /* ── If the caller is a board token and left ?dev= empty,
                   assume they want their own settings. ──────────────── */
            if (string.IsNullOrWhiteSpace(dev) && isDevice)
                dev = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(dev))
                return BadRequest("Query parameter 'dev' is required.");

            /* ── Does the device exist? ──────────────────────────────── */
            var device = await _devices.GetAsync(dev);
            if (device is null)
                return NotFound($"Device {dev} not found.");

            /* ── Authorisation matrix ───────────────────────────────── */
            bool isAdmin = User.IsInRole(UserRoles.Admin);

            bool isOwner = device.OwnerId != null &&
                           User.FindFirstValue(ClaimTypes.NameIdentifier) == device.OwnerId;

            bool isSameDevice = isDevice &&
                                string.Equals(User.Identity?.Name, dev,
                                              StringComparison.OrdinalIgnoreCase);

            if (!(isAdmin || isOwner || isSameDevice || isWorker))
                return Forbid();

            /* ── Fetch & return settings ────────────────────────────── */
            var s = await _settings.GetAsync(dev);
            return s is null ? NotFound() : Ok(s);
        }

        [HttpPut]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.User}")]
        public async Task<IActionResult> Put([FromQuery] string dev, SettingsDto dto)
        {
            var device = await _devices.GetAsync(dev);
            if (device == null) return NotFound();

            bool isAdmin = User.IsInRole(UserRoles.Admin);
            bool isOwner = device.OwnerId != null &&
                           User.FindFirstValue(ClaimTypes.NameIdentifier) == device.OwnerId;
            if (!isAdmin && !isOwner) return Forbid();

            var newSettings = new Settings
            {
                DeviceMac = dev,
                Watering = new Watering
                {
                    Manual = dto.Watering.Manual,
                    SoilMin = dto.Watering.SoilMin,
                    SoilMax = dto.Watering.SoilMax,
                    MaxPumpSeconds = dto.Watering.MaxPumpSeconds,
                    FertHours = dto.Watering.FertHours
                },
                Lighting = new Lighting
                {
                    Manual = dto.Lighting.Manual,
                    LuxLow = dto.Lighting.LuxLow,
                    OnHour = dto.Lighting.OnHour,
                    OffHour = dto.Lighting.OffHour
                },
                Security = new Security
                {
                    Armed = dto.Security.Armed,
                    AlarmStart = dto.Security.AlarmWindow.Start,
                    AlarmEnd = dto.Security.AlarmWindow.End
                },
                UpdatedAt = DateTime.UtcNow
            };
            await _settings.UpsertAsync(newSettings);
            await _settings.SaveAsync();
            return NoContent();
        }
    }
}
