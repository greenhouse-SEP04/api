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
    public class SettingsRepository : ISettingsRepository
    {
        private readonly AppDbContext _db;
        public SettingsRepository(AppDbContext db) => _db = db;

        public Task<Settings?> GetAsync(string mac) =>
            _db.SettingsSet.FirstOrDefaultAsync(s => s.DeviceMac == mac);

        public async Task UpsertAsync(Settings s)
        {
            var existing = await _db.SettingsSet.FirstOrDefaultAsync(x => x.DeviceMac == s.DeviceMac);
            if (existing == null) await _db.SettingsSet.AddAsync(s);
            else _db.Entry(existing).CurrentValues.SetValues(s);
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
