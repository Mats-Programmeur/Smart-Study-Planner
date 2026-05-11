using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartStudyPlanner.Api.Configuration;
using SmartStudyPlanner.Api.Data;
using SmartStudyPlanner.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";

builder.Services.AddDbContext<StudyPlannerDbContext>(options =>
{
    if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConnectionString = builder.Configuration.GetConnectionString("SqliteDb")
            ?? throw new InvalidOperationException("Connection string 'SqliteDb' is not configured.");

        options.UseSqlite(ResolveSqliteConnectionString(sqliteConnectionString, builder.Environment));
        return;
    }

    var sqlServerConnectionString = builder.Configuration.GetConnectionString("SmartStudyPlannerDb")
        ?? throw new InvalidOperationException("Connection string 'SmartStudyPlannerDb' is not configured.");

    options.UseSqlServer(sqlServerConnectionString);
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<PasswordHasherService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<DeadlineService>();
builder.Services.AddScoped<AdviceService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DatabaseInitializer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string ResolveSqliteConnectionString(string connectionString, IWebHostEnvironment environment)
{
    var marker = "Data Source=";

    if (!connectionString.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
    {
        return connectionString;
    }

    var configuredPath = connectionString[marker.Length..].Trim();

    if (Path.IsPathRooted(configuredPath))
    {
        var absoluteDirectory = Path.GetDirectoryName(configuredPath);

        if (!string.IsNullOrWhiteSpace(absoluteDirectory))
        {
            Directory.CreateDirectory(absoluteDirectory);
        }

        return connectionString;
    }

    var baseDirectory = environment.IsDevelopment()
        ? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartStudyPlanner")
        : AppContext.BaseDirectory;

    Directory.CreateDirectory(baseDirectory);

    var absolutePath = Path.Combine(baseDirectory, configuredPath);
    return $"{marker}{absolutePath}";
}
