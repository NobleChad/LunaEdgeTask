using AutoMapper;
using LunaEdgeTask.DTOS;
using LunaEdgeTask.Models;
using LunaEdgeTask.Services;
using Moq;
using Xunit;

namespace LunaEdgeTask.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepoMock;
        private readonly IMapper _mapper;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepoMock = new Mock<ITaskRepository>();

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<TaskItem>(It.IsAny<TaskDto>()))
                      .Returns((TaskDto dto) => new TaskItem
                      {
                          Title = dto.Title,
                          Description = dto.Description,
                          Priority = dto.Priority,
                          Status = dto.Status,
                          DueDate = dto.DueDate
                      });

            mapperMock.Setup(m => m.Map(It.IsAny<TaskDto>(), It.IsAny<TaskItem>()))
                      .Callback((TaskDto dto, TaskItem task) =>
                      {
                          task.Title = dto.Title;
                          task.Description = dto.Description;
                          task.Priority = dto.Priority;
                          task.Status = dto.Status;
                          task.DueDate = dto.DueDate;
                      });

            _mapper = mapperMock.Object;
            _taskService = new TaskService(_taskRepoMock.Object, _mapper);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldAddTask()
        {
            var userId = Guid.NewGuid();
            var dto = new TaskDto
            {
                Title = "Test Task",
                Description = "Task Description",
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var result = await _taskService.CreateTaskAsync(userId, dto);

            Assert.Equal(userId, result.UserId);
            Assert.Equal(dto.Title, result.Title);
            _taskRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
            _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTaskAsync_ShouldReturnTask_WhenExists()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, UserId = userId };

            _taskRepoMock.Setup(r => r.GetByIdAsync(taskId, userId))
                         .ReturnsAsync(task);

            var result = await _taskService.GetTaskAsync(userId, taskId);

            Assert.NotNull(result);
            Assert.Equal(taskId, result!.Id);
        }

        [Fact]
        public async Task GetTaskAsync_ShouldReturnNull_WhenNotExists()
        {
            _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                         .ReturnsAsync((TaskItem?)null);

            var result = await _taskService.GetTaskAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldUpdateTask_WhenExists()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = new TaskItem { Id = taskId, UserId = userId, Title = "Old Title" };
            var dto = new TaskDto { Title = "New Title" };

            _taskRepoMock.Setup(r => r.GetByIdAsync(taskId, userId))
                         .ReturnsAsync(existingTask);

            var result = await _taskService.UpdateTaskAsync(userId, taskId, dto);

            Assert.NotNull(result);
            Assert.Equal("New Title", result!.Title);
            _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldReturnNull_WhenTaskNotExists()
        {
            _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                         .ReturnsAsync((TaskItem?)null);

            var result = await _taskService.UpdateTaskAsync(Guid.NewGuid(), Guid.NewGuid(), new TaskDto());

            Assert.Null(result);
            _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnTrue_WhenTaskDeleted()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var task = new TaskItem { Id = taskId, UserId = userId };

            _taskRepoMock.Setup(r => r.GetByIdAsync(taskId, userId))
                         .ReturnsAsync(task);

            var result = await _taskService.DeleteTaskAsync(userId, taskId);

            Assert.True(result);
            _taskRepoMock.Verify(r => r.DeleteAsync(task), Times.Once);
            _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnFalse_WhenTaskNotExists()
        {
            _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                         .ReturnsAsync((TaskItem?)null);

            var result = await _taskService.DeleteTaskAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result);
            _taskRepoMock.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>()), Times.Never);
            _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetTasksAsync_ShouldReturnTasksAndTotal()
        {
            var userId = Guid.NewGuid();
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = Guid.NewGuid(), UserId = userId }
            };

            _taskRepoMock.Setup(r => r.GetTasksAsync(userId, null, null, null, null, null, null, 1, 10))
                         .ReturnsAsync(tasks);

            _taskRepoMock.Setup(r => r.CountTasksAsync(userId, null, null, null, null))
                         .ReturnsAsync(tasks.Count);

            var result = await _taskService.GetTasksAsync(userId, null, null, null, null, null, null, 1, 10);

            Assert.Equal(tasks.Count, result.Total);
            Assert.Equal(tasks, result.Tasks);
        }
    }
}
