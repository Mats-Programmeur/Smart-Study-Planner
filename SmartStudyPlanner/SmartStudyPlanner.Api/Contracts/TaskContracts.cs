namespace SmartStudyPlanner.Api.Contracts
{
    public record SaveTaskRequest(
        string Titel,
        string? Beschrijving,
        DateOnly Datum,
        TimeOnly StartTijd,
        TimeOnly EindTijd,
        string Prioriteit,
        int GeschatteStudietijdMinuten,
        string Status);
}
