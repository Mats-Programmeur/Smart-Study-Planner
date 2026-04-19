using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Data;
using SmartStudyPlanner.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("SmartStudyPlannerDb")
    ?? throw new InvalidOperationException("Connection string 'SmartStudyPlannerDb' is not configured.");

builder.Services.AddDbContext<StudyPlannerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<TaskService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StudyPlannerDbContext>();
    dbContext.Database.EnsureCreated();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
