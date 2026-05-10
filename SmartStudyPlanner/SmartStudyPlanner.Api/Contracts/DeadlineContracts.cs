namespace SmartStudyPlanner.Api.Contracts
{
    public record SaveDeadlineRequest(
        string Titel,
        string? Beschrijving,
        DateOnly Datum,
        TimeOnly EindTijd,
        string Prioriteit);
}
