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
    }
}
