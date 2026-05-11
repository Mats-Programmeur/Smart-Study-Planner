using System.Security.Claims;

namespace SmartStudyPlanner.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out var userId))
            {
                throw new InvalidOperationException("Er is geen geldige gebruikersidentiteit gevonden.");
            }

            return userId;
        }
    }
}
