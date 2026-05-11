namespace SmartStudyPlanner.Api.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public required string Naam { get; set; }
        public required string Email { get; set; }
        public required string WachtwoordHash { get; set; }
        public required string WachtwoordSalt { get; set; }
        public required string Rol { get; set; }
        public bool IsActief { get; set; } = true;
        public DateTime AangemaaktOpUtc { get; set; } = DateTime.UtcNow;
    }
}
