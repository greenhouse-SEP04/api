using api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Telemetry> Telemetries => Set<Telemetry>();
        public DbSet<Settings> SettingsSet => Set<Settings>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            b.Entity<Device>()
             .HasOne(d => d.CurrentSettings)
             .WithOne(s => s.Device)
             .HasForeignKey<Settings>(s => s.DeviceMac)
             .HasPrincipalKey<Device>(d => d.Mac);
            b.Entity<Device>()
             .HasMany(d => d.Telemetries)
             .WithOne(t => t.Device)
             .HasForeignKey(t => t.DeviceMac);
        }
    }
}
