using LunaEdgeTask.DTOS;
using LunaEdgeTask.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LunaEdgeTask.Controllers
{
    /// <summary>
    /// Handles task management operations such as creating, retrieving, updating, and deleting tasks.
    /// </summary>
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

        /// <summary>
        /// Creates a new task for the authenticated user.
        /// </summary>
        /// <param name="dto">The task details (title, description, priority, due date).</param>
        /// <param name="token">Optional JWT token passed via query string.</param>
        /// <returns>The created task.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskDto dto, [FromQuery] string? token)
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} creating task: {dto.Title}");
            var task = await _taskService.CreateTaskAsync(userId, dto);
            return Ok(task);
        }

        /// <summary>
        /// Retrieves tasks for the authenticated user with optional filters and pagination.
        /// </summary>
        /// <param name="status">Filter by task status.</param>
        /// <param name="priority">Filter by task priority.</param>
        /// <param name="dueFrom">Filter tasks due after this date.</param>
        /// <param name="dueTo">Filter tasks due before this date.</param>
        /// <param name="token">Optional JWT token passed via query string.</param>
        /// <param name="sortBy">Property to sort by (default: dueDate).</param>
        /// <param name="sortOrder">Sort order (asc or desc).</param>
        /// <param name="page">Page number (default: 1).</param>
        /// <param name="pageSize">Page size (default: 10).</param>
        /// <returns>Paged list of tasks.</returns>
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
            var (tasks, total) = await _taskService.GetTasksAsync(userId, status, priority, dueFrom, dueTo, sortBy, sortOrder, page, pageSize);
            return Ok(new { page, pageSize, total, tasks });
        }

        /// <summary>
        /// Retrieves a single task by its ID.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <param name="token">Optional JWT token passed via query string.</param>
        /// <returns>The task if found, otherwise 404.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id, [FromQuery] string? token)
        {
            var userId = GetUserId();
            var task = await _taskService.GetTaskAsync(userId, id);
            if (task == null)
                return NotFound();
            return Ok(task);
        }

        /// <summary>
        /// Updates an existing task by its ID.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <param name="dto">Updated task details.</param>
        /// <param name="token">Optional JWT token passed via query string.</param>
        /// <returns>The updated task if successful, otherwise 404.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskDto dto, [FromQuery] string? token)
        {
            var userId = GetUserId();
            var task = await _taskService.UpdateTaskAsync(userId, id, dto);
            if (task == null)
                return NotFound();
            return Ok(task);
        }

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <param name="token">Optional JWT token passed via query string.</param>
        /// <returns>No content if successful, otherwise 404.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id, [FromQuery] string? token)
        {
            var userId = GetUserId();
            var success = await _taskService.DeleteTaskAsync(userId, id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdStr);
        }
    }
}
