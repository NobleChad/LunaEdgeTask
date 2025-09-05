using LunaEdgeTask.Data;
using LunaEdgeTask.Models;
using Microsoft.EntityFrameworkCore;

namespace LunaEdgeTask.Services
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _db;
        public TaskRepository(AppDbContext db) => _db = db;

        public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        public async Task<List<TaskItem>> GetTasksAsync(Guid userId, Models.TaskStatus? status, TaskPriority? priority,
                                                        DateTime? dueFrom, DateTime? dueTo,
                                                        string sortBy, string sortOrder,
                                                        int page, int pageSize)
        {
            var query = _db.Tasks.Where(t => t.UserId == userId);

            if (status.HasValue) query = query.Where(t => t.Status == status);
            if (priority.HasValue) query = query.Where(t => t.Priority == priority);
            if (dueFrom.HasValue) query = query.Where(t => t.DueDate >= dueFrom);
            if (dueTo.HasValue) query = query.Where(t => t.DueDate <= dueTo);

            query = sortBy.ToLower() switch
            {
                "priority" => sortOrder == "desc" ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                _ => sortOrder == "desc" ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            };

            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<int> CountTasksAsync(Guid userId, Models.TaskStatus? status, TaskPriority? priority, DateTime? dueFrom, DateTime? dueTo)
        {
            var query = _db.Tasks.Where(t => t.UserId == userId);

            if (status.HasValue) query = query.Where(t => t.Status == status);
            if (priority.HasValue) query = query.Where(t => t.Priority == priority);
            if (dueFrom.HasValue) query = query.Where(t => t.DueDate >= dueFrom);
            if (dueTo.HasValue) query = query.Where(t => t.DueDate <= dueTo);

            return await query.CountAsync();
        }

        public async Task AddAsync(TaskItem task) => await _db.Tasks.AddAsync(task);

        public async Task DeleteAsync(TaskItem task) => _db.Tasks.Remove(task);

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }

}
