using LunaEdgeTask.Data;
using LunaEdgeTask.Models;
using LunaEdgeTask.Services;
using Microsoft.EntityFrameworkCore;

namespace LunaEdgeTask.Tests.Repositories
{
    public class TaskRepositoryTests
    {
        private static AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }

        private static User CreateUser(string username, string email) =>
            new User
            {
                Username = username,
                Email = email,
                PasswordHash = "testhash"
            };

        private static TaskItem CreateTask(User user, string title, string description, Models.TaskStatus status, TaskPriority priority, DateTime dueDate) =>
            new TaskItem
            {
                UserId = user.Id,
                Title = title,
                Description = description,
                Status = status,
                Priority = priority,
                DueDate = dueDate
            };

        [Fact]
        public async Task AddAsync_Should_Add_Task()
        {
            using var db = GetInMemoryDbContext();
            var user = CreateUser("user1", "user1@example.com");
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var task = CreateTask(user, "Test Task", "This is a test task", Models.TaskStatus.Pending, TaskPriority.Medium, DateTime.UtcNow.AddDays(1));

            await repo.AddAsync(task);
            await repo.SaveChangesAsync();

            var savedTask = await db.Tasks.FirstOrDefaultAsync(t => t.Title == "Test Task");
            Assert.NotNull(savedTask);
            Assert.Equal("This is a test task", savedTask.Description);
            Assert.Equal(user.Id, savedTask.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Task_If_Exists()
        {
            using var db = GetInMemoryDbContext();
            var user = CreateUser("user2", "user2@example.com");
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var task = CreateTask(user, "Find Me", "Find me description", Models.TaskStatus.Pending, TaskPriority.Low, DateTime.UtcNow);
            await db.Tasks.AddAsync(task);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var fetchedTask = await repo.GetByIdAsync(task.Id, user.Id);

            Assert.NotNull(fetchedTask);
            Assert.Equal("Find Me", fetchedTask.Title);
            Assert.Equal("Find me description", fetchedTask.Description);
        }

        [Fact]
        public async Task GetTasksAsync_Should_Filter_And_Sort_Tasks()
        {
            using var db = GetInMemoryDbContext();
            var user = CreateUser("user3", "user3@example.com");
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var tasks = new List<TaskItem>
            {
                CreateTask(user, "Task 1", "High priority task", Models.TaskStatus.Pending, TaskPriority.High, now.AddDays(3)),
                CreateTask(user, "Task 2", "Completed low priority", Models.TaskStatus.Completed, TaskPriority.Low, now.AddDays(1)),
                CreateTask(user, "Task 3", "Pending medium task", Models.TaskStatus.Pending, TaskPriority.Medium, now.AddDays(2)),
            };

            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);

            // Filter by status = Pending, sort by due date ascending
            var result = await repo.GetTasksAsync(user.Id, Models.TaskStatus.Pending, null, null, null, "duedate", "asc", 1, 10);

            Assert.Equal(2, result.Count);
            Assert.Equal("Task 3", result[0].Title);
            Assert.Equal("Task 1", result[1].Title);

            // Filter by priority = Low
            var lowPriority = await repo.GetTasksAsync(user.Id, null, TaskPriority.Low, null, null, "duedate", "asc", 1, 10);
            Assert.Single(lowPriority);
            Assert.Equal("Task 2", lowPriority[0].Title);
        }

        [Fact]
        public async Task CountTasksAsync_Should_Return_Correct_Count()
        {
            using var db = GetInMemoryDbContext();
            var user = CreateUser("user4", "user4@example.com");
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var tasks = new List<TaskItem>
            {
                CreateTask(user, "Task A", "Pending high",  Models.TaskStatus.Pending, TaskPriority.High, now.AddDays(1)),
                CreateTask(user, "Task B", "Pending low", Models.TaskStatus.Pending, TaskPriority.Low, now.AddDays(2)),
                CreateTask(user, "Task C", "Completed medium", Models.TaskStatus.Completed, TaskPriority.Medium, now.AddDays(3)),
            };

            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);

            var pendingCount = await repo.CountTasksAsync(user.Id, Models.TaskStatus.Pending, null, null, null);
            Assert.Equal(2, pendingCount);

            var lowPriorityCount = await repo.CountTasksAsync(user.Id, null, TaskPriority.Low, null, null);
            Assert.Equal(1, lowPriorityCount);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Task()
        {
            using var db = GetInMemoryDbContext();
            var user = CreateUser("user5", "user5@example.com");
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var task = CreateTask(user, "ToDelete", "Task to delete", Models.TaskStatus.Pending, TaskPriority.Medium, DateTime.UtcNow);
            await db.Tasks.AddAsync(task);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            await repo.DeleteAsync(task);
            await repo.SaveChangesAsync();

            var exists = await db.Tasks.AnyAsync(t => t.Id == task.Id);
            Assert.False(exists);
        }
    }
}
