using api.Data;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.Repositories.EfCore
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AppDbContext _db;
        public DeviceRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsAsync(string mac) =>
            _db.Devices.AnyAsync(d => d.Mac == mac);

        public Task<Device?> GetAsync(string mac) =>
            _db.Devices.Include(d => d.CurrentSettings)
                       .FirstOrDefaultAsync(d => d.Mac == mac);

        public async Task AddAsync(Device dev) => await _db.Devices.AddAsync(dev);

        public Task SaveAsync() => _db.SaveChangesAsync();

        public async Task<IReadOnlyList<Device>> GetByOwnerAsync(string ownerId)
        {
            return await _db.Devices.Where(d => d.OwnerId == ownerId).ToListAsync();
        }

        public async Task<IReadOnlyList<Device>> GetAllAsync()
        {
            return await _db.Devices.ToListAsync();
        }

        public async Task AssignAsync(Device dev, User owner)
        {
            dev.OwnerId = owner.Id;
            _db.Update(dev);
            await Task.CompletedTask;
        }

        public async Task UnassignAsync(Device dev)
        {
            dev.OwnerId = null;
            _db.Update(dev);
            await Task.CompletedTask;
        }

        public Task DeleteAsync(Device dev)
        {
            _db.Devices.Remove(dev);
            return Task.CompletedTask;
        }



        public Task<bool> IsActiveAsync(string mac, TimeSpan since) =>
    _db.Telemetries.AnyAsync(t =>
        t.DeviceMac == mac && t.Timestamp >= DateTime.UtcNow.Subtract(since));

    }
}
