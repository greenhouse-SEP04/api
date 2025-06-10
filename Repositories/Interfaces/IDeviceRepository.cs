using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.Repositories.Interfaces
{
    public interface IDeviceRepository
    {
        Task<bool> ExistsAsync(string mac);
        Task<Device?> GetAsync(string mac);
        Task AddAsync(Device dev);
        Task SaveAsync();
    }
}
