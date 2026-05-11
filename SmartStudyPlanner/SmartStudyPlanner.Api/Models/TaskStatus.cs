namespace SmartStudyPlanner.Api.Models
{
    public static class TaskStatus
    {
        public const string Gepland = "Gepland";
        public const string Bezig = "Bezig";
        public const string Afgerond = "Afgerond";

        public static readonly string[] All = [Gepland, Bezig, Afgerond];
    }
}
