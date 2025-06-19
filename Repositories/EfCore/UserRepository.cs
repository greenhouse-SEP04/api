using api.Data;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.EfCore
{
    /// <summary>
    ///     Thin wrapper around ASP‑NET Identity's <see cref="UserManager{TUser}"/>
    ///     so the rest of the application can stay repository‑centric.
    ///     All mutations delegate to <see cref="UserManager{TUser}"/> so that
    ///     password hashing, security stamps, etc. are handled correctly.
    /// </summary>
    public sealed class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;          // only used for Queries
        private readonly UserManager<User> _mgr;    // used for CUD operations

        public UserRepository(AppDbContext db, UserManager<User> mgr)
        {
            _db = db;
            _mgr = mgr;
        }

        // ---------------------------------------------------------------------
        //  🔍  Read operations – use the DbContext directly for projection
        // ---------------------------------------------------------------------
        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
             return await _db.Users.ToListAsync();
        }

        public async Task<User?> GetAsync(string id)
        {
            // Use FindAsync if you just know the primary key
            return await _db.Users.FindAsync(id);
        }

        // ---------------------------------------------------------------------
        //  ✏️  Mutations – use UserManager so Identity bookkeeping is intact
        // ---------------------------------------------------------------------
        public async Task AddAsync(User user, string password)
        {
            var result = await _mgr.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                // You can wrap the IdentityErrors into an exception or bubble up –
                // here we just throw so the controller can 400‑out.
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task UpdateAsync(User user)
        {
            var result = await _mgr.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task DeleteAsync(User user)
        {
            var result = await _mgr.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
