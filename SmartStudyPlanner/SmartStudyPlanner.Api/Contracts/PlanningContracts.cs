using SmartStudyPlanner.Api.Models;

namespace SmartStudyPlanner.Api.Contracts
{
    public record PlanningOverviewResponse(
        IReadOnlyList<TaskItem> Taken,
        IReadOnlyList<DeadlineItem> Deadlines);

    public record AdviceResponse(
        string Melding,
        string Aanbeveling,
        string Ernstniveau,
        string Type);
}
