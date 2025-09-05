using LunaEdgeTask.Data;
using LunaEdgeTask.Models;
using LunaEdgeTask.Services;
using Microsoft.EntityFrameworkCore;

namespace LunaEdgeTask.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_Should_Add_User_To_Db()
        {
            using var db = GetInMemoryDbContext();
            var repo = new UserRepository(db);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword123"
            };

            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var addedUser = await db.Users.FirstOrDefaultAsync(u => u.Username == "testuser");

            Assert.NotNull(addedUser);
            Assert.Equal("test@example.com", addedUser.Email);
            Assert.Equal("hashedpassword123", addedUser.PasswordHash);
        }

        [Fact]
        public async Task GetByUsernameOrEmailAsync_Should_Return_User_When_Exists()
        {
            using var db = GetInMemoryDbContext();
            var repo = new UserRepository(db);

            var user = new User
            {
                Username = "johndoe",
                Email = "john@example.com",
                PasswordHash = "securehash"
            };
            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var byUsername = await repo.GetByUsernameOrEmailAsync("johndoe");
            var byEmail = await repo.GetByUsernameOrEmailAsync("john@example.com");

            Assert.NotNull(byUsername);
            Assert.NotNull(byEmail);
            Assert.Equal("johndoe", byUsername.Username);
            Assert.Equal("john@example.com", byEmail.Email);
        }

        [Fact]
        public async Task ExistsByUsernameOrEmailAsync_Should_Return_True_If_User_Exists()
        {
            using var db = GetInMemoryDbContext();
            var repo = new UserRepository(db);

            var user = new User
            {
                Username = "alice",
                Email = "alice@example.com",
                PasswordHash = "alicehash"
            };
            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var existsByUsername = await repo.ExistsByUsernameOrEmailAsync("alice", "nonexistent@example.com");
            var existsByEmail = await repo.ExistsByUsernameOrEmailAsync("nonexistent", "alice@example.com");
            var notExists = await repo.ExistsByUsernameOrEmailAsync("bob", "bob@example.com");

            Assert.True(existsByUsername);
            Assert.True(existsByEmail);
            Assert.False(notExists);
        }

        [Fact]
        public async Task SaveChangesAsync_Should_Persist_Changes()
        {
            using var db = GetInMemoryDbContext();
            var repo = new UserRepository(db);

            var user = new User
            {
                Username = "bob",
                Email = "bob@example.com",
                PasswordHash = "bobh123"
            };
            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            var exists = await db.Users.AnyAsync(u => u.Username == "bob");
            Assert.True(exists);
        }
    }
}
