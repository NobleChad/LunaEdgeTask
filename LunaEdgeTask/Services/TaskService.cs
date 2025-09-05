using AutoMapper;
using LunaEdgeTask.DTOS;
using LunaEdgeTask.Models;

namespace LunaEdgeTask.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public TaskService(ITaskRepository taskRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task<TaskItem> CreateTaskAsync(Guid userId, TaskDto dto)
        {
            // Map DTO → TaskItem entity and associate with the user
            var task = _mapper.Map<TaskItem>(dto);
            task.UserId = userId;

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();
            return task;
        }

        public async Task<(List<TaskItem> Tasks, int Total)> GetTasksAsync(
            Guid userId,
            Models.TaskStatus? status, TaskPriority? priority,
            DateTime? dueFrom, DateTime? dueTo,
            string sortBy, string sortOrder,
            int page, int pageSize)
        {
            // Fetch filtered, sorted, paginated tasks
            var tasks = await _taskRepository.GetTasksAsync(
                userId, status, priority, dueFrom, dueTo, sortBy, sortOrder, page, pageSize);

            // Get total count separately for pagination
            var total = await _taskRepository.CountTasksAsync(userId, status, priority, dueFrom, dueTo);

            return (tasks, total);
        }

        public async Task<TaskItem?> GetTaskAsync(Guid userId, Guid taskId) =>
            await _taskRepository.GetByIdAsync(taskId, userId);

        public async Task<TaskItem?> UpdateTaskAsync(Guid userId, Guid taskId, TaskDto dto)
        {
            // Ensure task exists and belongs to user
            var task = await _taskRepository.GetByIdAsync(taskId, userId);
            if (task == null) return null;

            // Map updated values onto the existing entity
            _mapper.Map(dto, task);
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(Guid userId, Guid taskId)
        {
            // Ensure task exists and belongs to user
            var task = await _taskRepository.GetByIdAsync(taskId, userId);
            if (task == null) return false;

            await _taskRepository.DeleteAsync(task);
            await _taskRepository.SaveChangesAsync();
            return true;
        }
    }
}
