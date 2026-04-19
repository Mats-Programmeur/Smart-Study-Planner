using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Data;
using SmartStudyPlanner.Api.Models;

namespace SmartStudyPlanner.Api.Services
{
    public class TaskService
    {
        private readonly StudyPlannerDbContext _dbContext;

        public TaskService(StudyPlannerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<TaskItem> GetAll()
        {
            return _dbContext.Tasks
                .AsNoTracking()
                .OrderBy(task => task.Datum)
                .ThenBy(task => task.StartTijd)
                .ThenBy(task => task.Titel)
                .ToList();
        }

        public TaskItem Add(TaskItem task)
        {
            _dbContext.Tasks.Add(task);
            _dbContext.SaveChanges();
            return task;
        }
    }
}
