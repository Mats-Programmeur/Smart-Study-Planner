using SmartStudyPlanner.Api.Models;

namespace SmartStudyPlanner.Api.Services
{
    public class TaskService
    {
        private static readonly List<TaskItem> tasks = new();

        public List<TaskItem> GetAll()
        {
            return tasks
                .OrderBy(task => task.Datum)
                .ThenBy(task => task.StartTijd)
                .ThenBy(task => task.Titel)
                .ToList();
        }

        public TaskItem Add(TaskItem task)
        {
            task.Id = tasks.Count + 1;
            tasks.Add(task);
            return task;
        }
    }
}
