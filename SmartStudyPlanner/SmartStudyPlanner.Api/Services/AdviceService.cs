using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Contracts;
using SmartStudyPlanner.Api.Data;

namespace SmartStudyPlanner.Api.Services
{
    public class AdviceService
    {
        private readonly StudyPlannerDbContext _dbContext;

        public AdviceService(StudyPlannerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AdviceResponse>> GetAdviceForUserAsync(int userId)
        {
            var tasks = await _dbContext.Tasks
                .AsNoTracking()
                .Where(task => task.UserId == userId)
                .ToListAsync();

            var deadlines = await _dbContext.Deadlines
                .AsNoTracking()
                .Where(deadline => deadline.UserId == userId)
                .ToListAsync();

            var advice = new List<AdviceResponse>();

            foreach (var dayGroup in tasks.GroupBy(task => task.Datum))
            {
                var totalMinutes = dayGroup.Sum(task => task.GeschatteStudietijdMinuten);

                if (totalMinutes > 240)
                {
                    advice.Add(new AdviceResponse(
                        $"Op {dayGroup.Key:dd-MM-yyyy} staat meer dan 4 uur aan taken gepland.",
                        "Verdeel een deel van de taken over een andere dag om overbelasting te voorkomen.",
                        "Hoog",
                        "Studiebelasting"));
                }
            }

            foreach (var task in tasks)
            {
                var relatedDeadline = deadlines
                    .Where(deadline => deadline.Datum >= task.Datum)
                    .OrderBy(deadline => deadline.Datum)
                    .ThenBy(deadline => deadline.EindTijd)
                    .FirstOrDefault();

                if (relatedDeadline is null)
                {
                    continue;
                }

                var taskStart = task.Datum.ToDateTime(task.StartTijd);
                var deadlineMoment = relatedDeadline.Datum.ToDateTime(relatedDeadline.EindTijd);

                if (deadlineMoment - taskStart < TimeSpan.FromHours(24))
                {
                    advice.Add(new AdviceResponse(
                        $"De taak '{task.Titel}' start minder dan 24 uur voor deadline '{relatedDeadline.Titel}'.",
                        "Plan deze taak eerder in zodat je meer ruimte houdt voor uitloop of herhaling.",
                        "Middel",
                        "Deadline"));
                }
            }

            foreach (var dayGroup in deadlines.GroupBy(deadline => deadline.Datum))
            {
                if (dayGroup.Count() > 2)
                {
                    advice.Add(new AdviceResponse(
                        $"Op {dayGroup.Key:dd-MM-yyyy} vallen meer dan 2 deadlines samen.",
                        "Begin eerder aan taken met hoge prioriteit en verspreid je werk over meerdere dagen.",
                        "Hoog",
                        "Deadlines"));
                }
            }

            return advice
                .DistinctBy(item => $"{item.Type}-{item.Melding}")
                .ToList();
        }
    }
}
