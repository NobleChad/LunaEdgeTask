using LunaEdgeTask.DTOS;
using LunaEdgeTask.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LunaEdgeTask.Controllers
{
    [ApiController]
    [Route("tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskDto dto, [FromQuery] string? token)
        {
            var userId = GetUserId();
            try
            {
                _logger.LogInformation($"User {userId} attempting to create task with title {dto.Title}");
                var task = await _taskService.CreateTaskAsync(userId, dto);
                _logger.LogInformation($"Task {task.Id} created successfully for user {userId}");
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating task for user {userId}");
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] Models.TaskStatus? status,
            [FromQuery] Models.TaskPriority? priority,
            [FromQuery] DateTime? dueFrom,
            [FromQuery] DateTime? dueTo,
            [FromQuery] string? token,
            [FromQuery] string sortBy = "dueDate",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            try
            {
                _logger.LogInformation($"User {userId} retrieving tasks with filters: " +
                    $"status={status}, " +
                    $"priority={priority}, " +
                    $"dueFrom={dueFrom}, " +
                    $"dueTo={dueTo}, " +
                    $"sortBy={sortBy}, " +
                    $"sortOrder={sortOrder}, " +
                    $"page={page}, " +
                    $"pageSize={pageSize}");
                var (tasks, total) = await _taskService.GetTasksAsync(userId, status, priority, dueFrom, dueTo, sortBy, sortOrder, page, pageSize);
                _logger.LogInformation($"User {userId} retrieved {tasks.Count()} tasks");
                return Ok(new { page, pageSize, total, tasks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasks for user {userId}");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id, [FromQuery] string? token)
        {
            var userId = GetUserId();
            try
            {
                _logger.LogInformation($"User {userId} retrieving task {id}");
                var task = await _taskService.GetTaskAsync(userId, id);
                if (task == null)
                {
                    _logger.LogWarning($"Task {id} not found for user {userId}");
                    return NotFound();
                }
                _logger.LogInformation($"Task {id} retrieved successfully for user {userId}");
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving task {id} for user {userId}");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskDto dto, [FromQuery] string? token)
        {
            var userId = GetUserId();
            try
            {
                _logger.LogInformation($"User {userId} attempting to update task {id} with title {dto.Title}");
                var task = await _taskService.UpdateTaskAsync(userId, id, dto);
                if (task == null)
                {
                    _logger.LogWarning($"Task {id} not found for user {userId} during update");
                    return NotFound();
                }
                _logger.LogInformation($"Task {id} updated successfully for user {userId}");
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task {id} for user {userId}");
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id, [FromQuery] string? token)
        {
            var userId = GetUserId();
            try
            {
                _logger.LogInformation($"User {userId} attempting to delete task {id}");
                var success = await _taskService.DeleteTaskAsync(userId, id);
                if (!success)
                {
                    _logger.LogWarning($"Task {id} not found for user {userId} during deletion");
                    return NotFound();
                }
                _logger.LogInformation($"Task {id} deleted successfully for user {userId}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task {id} for user {userId}");
                throw;
            }
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdStr);
        }
    }
}