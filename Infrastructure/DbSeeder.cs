using api.Authorization;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api.Infrastructure;

/// <summary>
///     Populates the database with a complete demo scenario:
///     • 1 Admin   (admin/Admin#123)
///     • 2 Users   (alice/Alice#123, bob/Bob#123)
///     • 4 Devices – some assigned, some un‑assigned
///     • Telemetry history so that two devices are "active" (≤1h) and two "inactive"
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var users = services.GetRequiredService<UserManager<User>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var wc = services.GetRequiredService<IOptions<WorkerCredentials>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>()
                              .CreateLogger("DbSeeder");

        // 1️⃣  Apply migrations ----------------------------------------------------------------
        await db.Database.MigrateAsync();

        // 2️⃣  Ensure roles --------------------------------------------------------------------
        foreach (var role in new[]
        {
            UserRoles.Admin, UserRoles.User, UserRoles.Device, UserRoles.Worker
        })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        }

        // 3️⃣  Seed built‑in accounts -----------------------------------------------------------
        await EnsureUserAsync(users, UserRoles.Admin, "admin", "Admin#123");
        await EnsureUserAsync(users, UserRoles.Worker, wc.Username, wc.Password);

        // 4️⃣  Demo regular users ---------------------------------------------------------------
        await EnsureUserAsync(users, UserRoles.User, "alice", "Alice#123");
        await EnsureUserAsync(users, UserRoles.User, "bob", "Bob#123");

        await db.SaveChangesAsync();   // make sure Ids are generated before we use them

        // lookup user Ids for assigning devices
        var aliceId = (await users.FindByNameAsync("alice"))!.Id;
        var bobId = (await users.FindByNameAsync("bob"))!.Id;

        // 5️⃣  Devices + Settings ---------------------------------------------------------------
        var demoDevices = new[]
{
    new { Mac = "AA:BB:CC:01", Name = "Greenhouse-A", OwnerId = (string?)aliceId, Active = true  },
    new { Mac = "AA:BB:CC:02", Name = "Lab-Sensor",   OwnerId = (string?)bobId,   Active = false },
    new { Mac = "AA:BB:CC:03", Name = "Field-Node-7", OwnerId = (string?)null,    Active = true  },
    new { Mac = "AA:BB:CC:04", Name = "Sandbox-Test", OwnerId = (string?)null,    Active = false },
};


        foreach (var d in demoDevices)
        {
            if (await db.Devices.AnyAsync(x => x.Mac == d.Mac)) continue;   // idempotent

            db.Devices.Add(new Device
            {
                Mac = d.Mac,
                Name = d.Name,
                OwnerId = d.OwnerId
            });

            db.SettingsSet.Add(new Settings
            {
                DeviceMac = d.Mac,
                Watering = new Watering { SoilMin = 30, SoilMax = 55, MaxPumpSeconds = 12, FertHours = 72 },
                Lighting = new Lighting { LuxLow = 1200, OnHour = 6, OffHour = 22 },
                Security = new Security { Armed = false }
            });
        }
        await db.SaveChangesAsync();

        // 6️⃣  Telemetry -----------------------------------------------------------------------
        DateTime now = DateTime.UtcNow;
        Random rnd = new();

        foreach (var d in demoDevices)
        {
            // already telemetry newer than 24h? -> skip
            bool anyRecent = await db.Telemetries.AnyAsync(t => t.DeviceMac == d.Mac && t.Timestamp >= now.AddHours(-24));
            if (anyRecent) continue;

            int historyHours = 48;
            for (int i = 0; i < historyHours; i++)
            {
                var ts = now.AddHours(-i);
                if (!d.Active && i == 0) ts = now.AddHours(-3); // make first sample 3 h old → inactive

                db.Telemetries.Add(new Telemetry
                {
                    DeviceMac = d.Mac,
                    Timestamp = ts,
                    Temperature = (short)(20 + MathF.Sin(i / 3f) * 5),
                    Humidity = (short)(50 + MathF.Cos(i / 4f) * 10),
                    Soil = (short)(40 + MathF.Sin(i / 5f) * 15),
                    Lux = (ushort)(800 + MathF.Sin(i / 2f) * 200),
                    Level = (ushort)rnd.Next(0, 100),
                    Motion = false,
                    Tamper = rnd.NextDouble() < 0.05,
                    AccelX = 0,
                    AccelY = 0,
                    AccelZ = 0,
                    CfgRev = "seed‑1"
                });
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Demo seeding completed");
    }

    // internal helper -----------------------------------------------------------
    private static async Task EnsureUserAsync(UserManager<User> mgr, string role, string username, string password)
    {
        var user = await mgr.FindByNameAsync(username);
        if (user is null)
        {
            user = new User { UserName = username };
            await mgr.CreateAsync(user, password);
            await mgr.AddToRoleAsync(user, role);
        }
    }
}
