using LunaEdgeTask.Models;

namespace LunaEdgeTask.Services
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(Guid id, Guid userId);
        Task<List<TaskItem>> GetTasksAsync(Guid userId, Models.TaskStatus? status, TaskPriority? priority,
                                           DateTime? dueFrom, DateTime? dueTo,
                                           string sortBy, string sortOrder,
                                           int page, int pageSize);
        Task<int> CountTasksAsync(Guid userId, Models.TaskStatus? status, TaskPriority? priority, DateTime? dueFrom, DateTime? dueTo);
        Task AddAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
        Task SaveChangesAsync();
    }

}
