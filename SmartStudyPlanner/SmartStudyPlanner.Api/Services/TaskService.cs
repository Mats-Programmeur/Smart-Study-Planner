using SmartStudyPlanner.Api.Models;

namespace SmartStudyPlanner.Api.Services
{
    public class TaskService
    {
        private static readonly List<TaskItem> tasks = new();

        public List<TaskItem> GetAll()
        {
            return tasks;
        }

        public void Add(TaskItem task)
        {
            task.Id = tasks.Count + 1;
            tasks.Add(task);
        }
    }
}
