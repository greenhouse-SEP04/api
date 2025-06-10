using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.Repositories.Interfaces
{
    public interface ITelemetryRepository
    {
        Task AddAsync(Telemetry t);
        Task<IReadOnlyList<Telemetry>> GetLatestAsync(string mac, int limit);
        Task SaveAsync();
    }
}
