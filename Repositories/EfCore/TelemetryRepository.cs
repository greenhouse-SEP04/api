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
    public class TelemetryRepository : ITelemetryRepository
    {
        private readonly AppDbContext _db;
        public TelemetryRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Telemetry t) => await _db.Telemetries.AddAsync(t);

        public async Task<IReadOnlyList<Telemetry>> GetLatestAsync(string mac, int limit) =>
            await _db.Telemetries.Where(t => t.DeviceMac == mac)
                                 .OrderByDescending(t => t.Timestamp)
                                 .Take(limit)
                                 .ToListAsync();

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
