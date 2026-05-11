using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Data;
using SmartStudyPlanner.Api.Models;

namespace SmartStudyPlanner.Api.Services
{
    public class DatabaseInitializer
    {
        private readonly StudyPlannerDbContext _dbContext;
        private readonly UserService _userService;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            StudyPlannerDbContext dbContext,
            UserService userService,
            ILogger<DatabaseInitializer> logger)
        {
            _dbContext = dbContext;
            _userService = userService;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await _dbContext.Database.EnsureCreatedAsync();
            if (_dbContext.Database.IsSqlServer())
            {
                await EnsureCompatibilitySchemaAsync();
            }
            await _userService.EnsureSeedUsersAsync();
            if (_dbContext.Database.IsSqlServer())
            {
                await BackfillExistingTasksAsync();
            }
        }

        private async Task EnsureCompatibilitySchemaAsync()
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[Users]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Users] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Naam] NVARCHAR(80) NOT NULL,
                        [Email] NVARCHAR(200) NOT NULL,
                        [WachtwoordHash] NVARCHAR(512) NOT NULL,
                        [WachtwoordSalt] NVARCHAR(256) NOT NULL,
                        [Rol] NVARCHAR(30) NOT NULL,
                        [IsActief] BIT NOT NULL CONSTRAINT [DF_Users_IsActief] DEFAULT(1),
                        [AangemaaktOpUtc] DATETIME2 NOT NULL CONSTRAINT [DF_Users_AangemaaktOpUtc] DEFAULT SYSUTCDATETIME()
                    );
                    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users]([Email]);
                END
                """);

            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[Deadlines]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Deadlines] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Titel] NVARCHAR(120) NOT NULL,
                        [Beschrijving] NVARCHAR(500) NULL,
                        [Datum] DATE NOT NULL,
                        [EindTijd] TIME NOT NULL,
                        [Prioriteit] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Deadlines_Prioriteit] DEFAULT N'Normaal',
                        [UserId] INT NOT NULL
                    );
                END
                """);

            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[Tasks]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Tasks] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Titel] NVARCHAR(120) NOT NULL,
                        [Beschrijving] NVARCHAR(500) NULL,
                        [Datum] DATE NOT NULL,
                        [StartTijd] TIME NOT NULL,
                        [EindTijd] TIME NOT NULL,
                        [Prioriteit] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Tasks_Prioriteit] DEFAULT N'Normaal',
                        [GeschatteStudietijdMinuten] INT NOT NULL CONSTRAINT [DF_Tasks_GeschatteStudietijdMinuten] DEFAULT 60,
                        [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Tasks_Status] DEFAULT N'Gepland',
                        [UserId] INT NULL
                    );
                END
                """);

            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                IF COL_LENGTH('Tasks', 'Prioriteit') IS NULL
                    ALTER TABLE [Tasks] ADD [Prioriteit] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Tasks_Prioriteit_Compat] DEFAULT N'Normaal';

                IF COL_LENGTH('Tasks', 'GeschatteStudietijdMinuten') IS NULL
                    ALTER TABLE [Tasks] ADD [GeschatteStudietijdMinuten] INT NOT NULL CONSTRAINT [DF_Tasks_GeschatteStudietijdMinuten_Compat] DEFAULT 60;

                IF COL_LENGTH('Tasks', 'Status') IS NULL
                    ALTER TABLE [Tasks] ADD [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Tasks_Status_Compat] DEFAULT N'Gepland';

                IF COL_LENGTH('Tasks', 'UserId') IS NULL
                    ALTER TABLE [Tasks] ADD [UserId] INT NULL;
                """);

            _logger.LogInformation("Databasecompatibiliteit gecontroleerd voor Smart Study Planner.");
        }

        private async Task BackfillExistingTasksAsync()
        {
            var studentId = await _dbContext.Users
                .Where(user => user.Rol == RoleNames.Student)
                .OrderBy(user => user.Id)
                .Select(user => user.Id)
                .FirstOrDefaultAsync();

            if (studentId == 0)
            {
                return;
            }

            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE [Tasks] SET [UserId] = {studentId} WHERE [UserId] IS NULL");
        }
    }
}
