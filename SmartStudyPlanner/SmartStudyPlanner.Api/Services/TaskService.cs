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

        public async Task<List<TaskItem>> GetAllForUserAsync(int userId)
        {
            return await _dbContext.Tasks
                .AsNoTracking()
                .Where(task => task.UserId == userId)
                .OrderBy(task => task.Datum)
                .ThenBy(task => task.StartTijd)
                .ThenBy(task => task.Titel)
                .ToListAsync();
        }

        public async Task<TaskItem> AddAsync(TaskItem task)
        {
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> GetByIdForUserAsync(int taskId, int userId)
        {
            return await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == taskId && task.UserId == userId);
        }

        public async Task<TaskItem> UpdateAsync(TaskItem task)
        {
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task DeleteAsync(TaskItem task)
        {
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();
        }
    }
}
