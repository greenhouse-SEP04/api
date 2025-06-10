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
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] string dev)
        {
            var s = await _settings.GetAsync(dev);
            return s == null ? NotFound() : Ok(s);
        }

        [HttpPut]
        [Authorize]
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
                WateringManual = dto.WateringManual,
                SoilMin = dto.SoilMin,
                SoilMax = dto.SoilMax,
                FertHours = dto.FertHours,
                LightingManual = dto.LightingManual,
                LuxLow = dto.LuxLow,
                OnHour = dto.OnHour,
                OffHour = dto.OffHour
            };
            await _settings.UpsertAsync(newSettings);
            await _settings.SaveAsync();
            return NoContent();
        }
    }
}
