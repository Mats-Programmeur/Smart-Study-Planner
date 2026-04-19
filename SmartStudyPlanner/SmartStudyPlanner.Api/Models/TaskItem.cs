namespace SmartStudyPlanner.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public required string Titel { get; set; }
        public string? Beschrijving { get; set; }
        public DateOnly Datum { get; set; }
        public TimeOnly StartTijd { get; set; }
        public TimeOnly EindTijd { get; set; }
    }
}
