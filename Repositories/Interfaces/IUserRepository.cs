using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<User>> GetAllAsync();
        Task<User?> GetAsync(string id);
        Task AddAsync(User user, string password);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}
