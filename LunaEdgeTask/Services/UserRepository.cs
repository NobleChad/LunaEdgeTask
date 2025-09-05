using LunaEdgeTask.Data;
using LunaEdgeTask.Models;
using Microsoft.EntityFrameworkCore;

namespace LunaEdgeTask.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail) =>
            await _db.Users.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

        public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email) =>
            await _db.Users.AnyAsync(u => u.Username == username || u.Email == email);

        public async Task AddAsync(User user) => await _db.Users.AddAsync(user);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }

}
