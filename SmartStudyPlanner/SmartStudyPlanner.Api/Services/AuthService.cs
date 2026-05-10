using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Contracts;
using SmartStudyPlanner.Api.Data;
using SmartStudyPlanner.Api.Models;
using System.Net;
using System.Net.Mail;

namespace SmartStudyPlanner.Api.Services
{
    public class AuthService
    {
        private readonly StudyPlannerDbContext _dbContext;
        private readonly PasswordHasherService _passwordHasher;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            StudyPlannerDbContext dbContext,
            PasswordHasherService passwordHasher,
            JwtService jwtService,
            ILogger<AuthService> logger)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage, AuthResponse? Response)> RegisterAsync(RegisterRequest request)
        {
            var naam = request.Naam?.Trim();
            var email = request.Email?.Trim().ToLowerInvariant();
            var wachtwoord = request.Wachtwoord?.Trim();

            if (string.IsNullOrWhiteSpace(naam) || naam.Length > 80)
            {
                return (false, "Naam is verplicht en mag maximaal 80 tekens bevatten.", null);
            }

            if (!IsValidEmail(email))
            {
                return (false, "Vul een geldig e-mailadres in.", null);
            }

            if (string.IsNullOrWhiteSpace(wachtwoord) || wachtwoord.Length < 8)
            {
                return (false, "Wachtwoord moet minimaal 8 tekens bevatten.", null);
            }

            if (await _dbContext.Users.AnyAsync(user => user.Email == email))
            {
                return (false, "Er bestaat al een account met dit e-mailadres.", null);
            }

            var (hash, salt) = _passwordHasher.HashPassword(wachtwoord);

            var user = new UserAccount
            {
                Naam = WebUtility.HtmlEncode(naam),
                Email = email!,
                WachtwoordHash = hash,
                WachtwoordSalt = salt,
                Rol = RoleNames.Student,
                IsActief = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Nieuw studentaccount geregistreerd voor {Email}.", user.Email);

            return (true, null, CreateAuthResponse(user));
        }

        public async Task<(bool Success, string? ErrorMessage, AuthResponse? Response)> LoginAsync(LoginRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            var wachtwoord = request.Wachtwoord?.Trim();

            if (!IsValidEmail(email) || string.IsNullOrWhiteSpace(wachtwoord))
            {
                return (false, "Ongeldige inloggegevens.", null);
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(existingUser => existingUser.Email == email);

            if (user is null)
            {
                _logger.LogWarning("Mislukte loginpoging voor onbekend account {Email}.", email);
                return (false, "Ongeldige inloggegevens.", null);
            }

            if (!user.IsActief)
            {
                _logger.LogWarning("Login geweigerd voor gedeactiveerd account {Email}.", user.Email);
                return (false, "Dit account is gedeactiveerd.", null);
            }

            if (!_passwordHasher.VerifyPassword(wachtwoord, user.WachtwoordHash, user.WachtwoordSalt))
            {
                _logger.LogWarning("Mislukte loginpoging voor {Email}: onjuist wachtwoord.", user.Email);
                return (false, "Ongeldige inloggegevens.", null);
            }

            _logger.LogInformation("Gebruiker {Email} is succesvol ingelogd.", user.Email);

            return (true, null, CreateAuthResponse(user));
        }

        public async Task<UserAccount?> GetByIdAsync(int userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == userId);
        }

        private AuthResponse CreateAuthResponse(UserAccount user)
        {
            var token = _jwtService.GenerateToken(user);
            return new AuthResponse(token, user.Naam, user.Email, user.Rol);
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
