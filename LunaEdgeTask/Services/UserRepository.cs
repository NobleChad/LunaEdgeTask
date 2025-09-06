using LunaEdgeTask.Data;
using LunaEdgeTask.Models;
using Microsoft.EntityFrameworkCore;

namespace LunaEdgeTask.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _db.Users
                .Where(u => u.Username == usernameOrEmail)
                .FirstOrDefaultAsync()
                ?? await _db.Users
                .Where(u => u.Email == usernameOrEmail)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email)
        {
            return await _db.Users.AnyAsync(u => u.Username == username)
                || await _db.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user) => await _db.Users.AddAsync(user);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }

}
