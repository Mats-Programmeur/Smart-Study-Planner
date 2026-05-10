using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Contracts;
using SmartStudyPlanner.Api.Data;
using SmartStudyPlanner.Api.Models;
using System.Net;

namespace SmartStudyPlanner.Api.Services
{
    public class UserService
    {
        private readonly StudyPlannerDbContext _dbContext;
        private readonly PasswordHasherService _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(
            StudyPlannerDbContext dbContext,
            PasswordHasherService passwordHasher,
            ILogger<UserService> logger)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<List<UserSummaryResponse>> GetAllAsync()
        {
            return await _dbContext.Users
                .AsNoTracking()
                .OrderBy(user => user.Naam)
                .Select(user => new UserSummaryResponse(
                    user.Id,
                    user.Naam,
                    user.Email,
                    user.Rol,
                    user.IsActief,
                    user.AangemaaktOpUtc))
                .ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int userId, UpdateUserRequest request, int actingUserId)
        {
            if (request.Rol != RoleNames.Student && request.Rol != RoleNames.Beheerder)
            {
                return (false, "Rol moet Student of Beheerder zijn.");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(existingUser => existingUser.Id == userId);

            if (user is null)
            {
                return (false, "Gebruiker niet gevonden.");
            }

            if (user.Id == actingUserId && !request.IsActief)
            {
                return (false, "Je kunt je eigen account niet deactiveren.");
            }

            user.Rol = request.Rol;
            user.IsActief = request.IsActief;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Gebruiker {UserId} is bijgewerkt door beheerder {ActingUserId}. Nieuwe rol: {Role}, actief: {IsActive}.",
                userId,
                actingUserId,
                request.Rol,
                request.IsActief);

            return (true, null);
        }

        public async Task EnsureSeedUsersAsync()
        {
            if (await _dbContext.Users.AnyAsync())
            {
                return;
            }

            var (studentHash, studentSalt) = _passwordHasher.HashPassword("Student123!");
            var (adminHash, adminSalt) = _passwordHasher.HashPassword("Admin123!");

            _dbContext.Users.AddRange(
                new UserAccount
                {
                    Naam = WebUtility.HtmlEncode("Demo Student"),
                    Email = "student@smartstudyplanner.local",
                    WachtwoordHash = studentHash,
                    WachtwoordSalt = studentSalt,
                    Rol = RoleNames.Student,
                    IsActief = true
                },
                new UserAccount
                {
                    Naam = WebUtility.HtmlEncode("Demo Beheerder"),
                    Email = "admin@smartstudyplanner.local",
                    WachtwoordHash = adminHash,
                    WachtwoordSalt = adminSalt,
                    Rol = RoleNames.Beheerder,
                    IsActief = true
                });

            await _dbContext.SaveChangesAsync();
        }
    }
}
