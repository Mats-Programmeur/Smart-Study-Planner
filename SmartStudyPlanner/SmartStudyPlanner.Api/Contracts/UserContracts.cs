namespace SmartStudyPlanner.Api.Contracts
{
    public record UserSummaryResponse(
        int Id,
        string Naam,
        string Email,
        string Rol,
        bool IsActief,
        DateTime AangemaaktOpUtc);

    public record UpdateUserRequest(string Rol, bool IsActief);
}
