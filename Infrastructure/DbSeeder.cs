using api.Authorization;
using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        // resolve everything we need
        var db = services.GetRequiredService<AppDbContext>();        // your DbContext
        var users = services.GetRequiredService<UserManager<User>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var wc = services.GetRequiredService<IOptions<WorkerCredentials>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>()
                                .CreateLogger("DbSeeder");

        /* 1) Apply pending migrations */
        await db.Database.MigrateAsync();

        /* 2) Ensure roles exist */
        foreach (var role in new[]
        {
            UserRoles.Admin, UserRoles.User, UserRoles.Device, UserRoles.Worker
        })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        }

        /* 3) Seed ADMIN account (demo values – move to secret store) */
        const string adminUser = "admin";
        const string adminPwd = "Admin#123";   // change in real deployments

        if (await users.FindByNameAsync(adminUser) is null)
        {
            var admin = new User { UserName = adminUser };
            await users.CreateAsync(admin, adminPwd);
            await users.AddToRoleAsync(admin, UserRoles.Admin);
            logger.LogInformation("Created default admin user");
        }

        /* 4) Seed WORKER account from WorkerCredentials */
        if (await users.FindByNameAsync(wc.Username) is null)
        {
            var worker = new User { UserName = wc.Username };
            await users.CreateAsync(worker, wc.Password);
            await users.AddToRoleAsync(worker, UserRoles.Worker);
            logger.LogInformation("Created Worker user '{user}'", wc.Username);
        }

        /* 5) Seed demo device + settings (optional) */
        const string mac = "AA:BB:CC:DD:EE:FF";

        if (!await db.Devices.AnyAsync(d => d.Mac == mac))
        {
            db.Devices.Add(new Device
            {
                Mac = mac,
                Name = "Greenhouse-Demo"
            });

            db.SettingsSet.Add(new Settings    // <— use the correct DbSet
            {
                DeviceMac = mac,
                Watering = new Watering { SoilMin = 30, SoilMax = 50, MaxPumpSeconds = 10, FertHours = 72 },
                Lighting = new Lighting { LuxLow = 1500, OnHour = 6, OffHour = 22 },
                Security = new Security { Armed = false }
            });

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded demo device {mac}", mac);
        }
    }
}
