namespace SmartStudyPlanner.Api.Models
{
    public static class TaskPriority
    {
        public const string Laag = "Laag";
        public const string Normaal = "Normaal";
        public const string Hoog = "Hoog";

        public static readonly string[] All = [Laag, Normaal, Hoog];
    }
}
