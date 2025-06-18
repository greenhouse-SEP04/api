using api.Authorization;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("v1/device")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceRepository _repo;
    private readonly IUserRepository _users;
    private readonly ITelemetryRepository _tele;
    public DeviceController(IDeviceRepository r, IUserRepository u, ITelemetryRepository t)
    { _repo = r; _users = u; _tele = t; }

    // ─────────────────────────────────────────────────────────────
    // 1. LIST
    //    • Admin gets everything
    //    • Regular user gets only own devices
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List()
    {
        if (User.IsInRole(UserRoles.Admin))
            return Ok(await _repo.GetAllAsync());

        var myId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _repo.GetByOwnerAsync(myId));
    }

    // ─────────────────────────────────────────────────────────────
    // 2. ASSIGN  (Admin only)
    [HttpPost("{mac}/assign/{userId}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Assign(string mac, string userId)
    {
        var dev = await _repo.GetAsync(mac);
        var user = await _users.GetAsync(userId);
        if (dev == null || user == null) return NotFound();

        await _repo.AssignAsync(dev, user);
        await _repo.SaveAsync();
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────
    // 3. DELETE  (Admin or owner)
    [HttpDelete("{mac}")]
    [Authorize]
    public async Task<IActionResult> Delete(string mac)
    {
        var dev = await _repo.GetAsync(mac);
        if (dev == null) return NotFound();

        bool admin = User.IsInRole(UserRoles.Admin);
        bool owner = dev.OwnerId != null &&
                     User.FindFirstValue(ClaimTypes.NameIdentifier) == dev.OwnerId;
        if (!admin && !owner) return Forbid();

        await _repo.DeleteAsync(dev);
        await _repo.SaveAsync();
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────
    // 4. ACTIVE?   – last telemetry within 1 h
    [HttpGet("{mac}/active")]
    [Authorize]
    public async Task<IActionResult> IsActive(string mac)
    {
        var dev = await _repo.GetAsync(mac);
        if (dev == null) return NotFound();

        bool adminOrOwner =
            User.IsInRole(UserRoles.Admin) ||
            User.FindFirstValue(ClaimTypes.NameIdentifier) == dev.OwnerId;
        if (!adminOrOwner) return Forbid();

        bool active = await _repo.IsActiveAsync(mac, TimeSpan.FromHours(1));
        return Ok(new { active });
    }

    // ─────────────────────────────────────────────────────────────
    // 5. TELEMETRY  – owners & admins
    [HttpGet("{mac}/telemetry")]
    [Authorize]
    public async Task<IActionResult> Telemetry(string mac, int limit = 100)
    {
        var dev = await _repo.GetAsync(mac);
        if (dev == null) return NotFound();

        bool adminOrOwner =
            User.IsInRole(UserRoles.Admin) ||
            User.FindFirstValue(ClaimTypes.NameIdentifier) == dev.OwnerId;
        if (!adminOrOwner) return Forbid();

        var data = await _tele.GetLatestAsync(mac, limit);
        return Ok(data);
    }
}
