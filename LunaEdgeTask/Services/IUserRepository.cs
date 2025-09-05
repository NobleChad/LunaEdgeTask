using LunaEdgeTask.Models;

namespace LunaEdgeTask.Services
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> ExistsByUsernameOrEmailAsync(string username, string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
