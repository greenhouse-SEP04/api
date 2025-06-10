using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.Repositories.Interfaces
{
    public interface ISettingsRepository
    {
        Task<Settings?> GetAsync(string mac);
        Task UpsertAsync(Settings s);
        Task SaveAsync();
    }
}
