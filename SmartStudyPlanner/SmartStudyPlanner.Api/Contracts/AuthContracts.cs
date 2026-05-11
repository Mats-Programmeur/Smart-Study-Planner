namespace SmartStudyPlanner.Api.Contracts
{
    public record RegisterRequest(string Naam, string Email, string Wachtwoord);

    public record LoginRequest(string Email, string Wachtwoord);

    public record AuthResponse(string Token, string Naam, string Email, string Rol);
}
