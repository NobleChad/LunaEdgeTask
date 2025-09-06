using AutoMapper;
using LunaEdgeTask.DTOS;
using LunaEdgeTask.Models;
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
        private readonly IMapper _mapper;

        public TasksController(TaskService taskService, ILogger<TasksController> logger, IMapper mapper)
        {
            _taskService = taskService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new task for the authenticated user.
        /// </summary>
        /// <param name="dto">The task details (title, description, priority, due date).</param>
        /// <response code="200">Task created successfully.</response>
        /// <response code="400">Validation failed or invalid data provided.</response>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskDto dto, [FromQuery] string? token)
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} creating task: {dto.Title}");
            var task = await _taskService.CreateTaskAsync(userId, dto);
            var taskDto = _mapper.Map<TaskResponseDto>(task);
            return Ok(taskDto);
        }

        /// <summary>
        /// Retrieves tasks for the authenticated user with optional filters and pagination.
        /// </summary>
        /// <param name="status">Filter by task status.</param>
        /// <param name="priority">Filter by task priority.</param>
        /// <param name="dueFrom">Filter tasks due after this date.</param>
        /// <param name="dueTo">Filter tasks due before this date.</param>
        /// <param name="sortBy">Property to sort by (default: dueDate).</param>
        /// <param name="sortOrder">Sort order (asc or desc).</param>
        /// <param name="page">Page number (default: 1).</param>
        /// <param name="pageSize">Page size (default: 10).</param>
        /// <returns>Paged list of tasks (DTOs).</returns>
        /// <response code="200">Tasks retrieved successfully (may be empty).</response>
        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] Models.TaskStatus? status,
            [FromQuery] TaskPriority? priority,
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

            if (tasks.Count == 0)
                return Ok(new { message = "No tasks found.", page, pageSize, total, tasks = new List<TaskResponseDto>() });

            var taskDtos = _mapper.Map<List<TaskResponseDto>>(tasks);
            return Ok(new { page, pageSize, total, tasks = taskDtos });
        }

        /// <summary>
        /// Retrieves a single task by its ID.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <returns>The task (DTO) if found, otherwise 404.</returns>
        /// <response code="200">Task retrieved successfully.</response>
        /// <response code="404">Task not found or user not authorized.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id, [FromQuery] string? token)
        {
            var userId = GetUserId();
            var task = await _taskService.GetTaskAsync(userId, id);
            if (task == null)
                return NotFound(new { message = "Task not found or you are not authorized to view it." });

            var taskDto = _mapper.Map<TaskResponseDto>(task);
            return Ok(taskDto);
        }

        /// <summary>
        /// Updates an existing task by its ID.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <param name="dto">Updated task details.</param>
        /// <returns>The updated task (DTO) if successful, otherwise 404.</returns>
        /// <response code="200">Task updated successfully.</response>
        /// <response code="400">Validation failed or invalid data provided.</response>
        /// <response code="404">Task not found or user not authorized.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskDto dto, [FromQuery] string? token)
        {
            var userId = GetUserId();
            var task = await _taskService.UpdateTaskAsync(userId, id, dto);
            if (task == null)
                return NotFound(new { message = "Task not found or you are not authorized to update it." });

            var taskDto = _mapper.Map<TaskResponseDto>(task);
            return Ok(taskDto);
        }

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <returns>No content if successful, otherwise 404.</returns>
        /// <response code="200">Task deleted successfully.</response>
        /// <response code="404">Task not found or user not authorized.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id, [FromQuery] string? token)
        {
            var userId = GetUserId();
            var success = await _taskService.DeleteTaskAsync(userId, id);
            if (!success)
                return NotFound(new { message = "Task not found or you are not authorized to delete it." });

            return Ok(new { message = "Task deleted successfully." });
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdStr);
        }
    }
}
