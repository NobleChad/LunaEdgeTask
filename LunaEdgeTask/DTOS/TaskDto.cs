using LunaEdgeTask.Models;

namespace LunaEdgeTask.DTOS
{
    public class TaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public Models.TaskStatus Status { get; set; }
    }
}
