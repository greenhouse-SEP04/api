using api.Models;

public interface IDeviceRepository
{
    /* ── read/query ─────────────────────────────── */
    Task<bool> ExistsAsync(string mac);
    Task<Device?> GetAsync(string mac);
    Task<IReadOnlyList<Device>> GetByOwnerAsync(string ownerId);
    Task<IReadOnlyList<Device>> GetAllAsync();

    /* ── write/ mutate ──────────────────────────── */
    Task AddAsync(Device dev);                       // ← NEW
    Task AssignAsync(Device dev, User owner);
    Task UnassignAsync(Device dev);
    Task DeleteAsync(Device dev);

    /* ── utility ────────────────────────────────── */
    Task<bool> IsActiveAsync(string mac, TimeSpan since);
    Task SaveAsync();
}
