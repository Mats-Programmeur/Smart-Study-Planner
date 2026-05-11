using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Data;
using SmartStudyPlanner.Api.Models;

namespace SmartStudyPlanner.Api.Services
{
    public class DeadlineService
    {
        private readonly StudyPlannerDbContext _dbContext;

        public DeadlineService(StudyPlannerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DeadlineItem>> GetAllForUserAsync(int userId)
        {
            return await _dbContext.Deadlines
                .AsNoTracking()
                .Where(deadline => deadline.UserId == userId)
                .OrderBy(deadline => deadline.Datum)
                .ThenBy(deadline => deadline.EindTijd)
                .ThenBy(deadline => deadline.Titel)
                .ToListAsync();
        }

        public async Task<DeadlineItem?> GetByIdForUserAsync(int deadlineId, int userId)
        {
            return await _dbContext.Deadlines
                .FirstOrDefaultAsync(deadline => deadline.Id == deadlineId && deadline.UserId == userId);
        }

        public async Task<DeadlineItem> AddAsync(DeadlineItem deadline)
        {
            _dbContext.Deadlines.Add(deadline);
            await _dbContext.SaveChangesAsync();
            return deadline;
        }

        public async Task<DeadlineItem> UpdateAsync(DeadlineItem deadline)
        {
            _dbContext.Deadlines.Update(deadline);
            await _dbContext.SaveChangesAsync();
            return deadline;
        }

        public async Task DeleteAsync(DeadlineItem deadline)
        {
            _dbContext.Deadlines.Remove(deadline);
            await _dbContext.SaveChangesAsync();
        }
    }
}
