using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Models;

namespace SmartStudyPlanner.Api.Data
{
    public class StudyPlannerDbContext(DbContextOptions<StudyPlannerDbContext> options) : DbContext(options)
    {
        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("Tasks");
                entity.HasKey(task => task.Id);
                entity.Property(task => task.Titel)
                    .IsRequired()
                    .HasMaxLength(120);
                entity.Property(task => task.Beschrijving)
                    .HasMaxLength(500);
                entity.Property(task => task.Datum)
                    .IsRequired();
                entity.Property(task => task.StartTijd)
                    .IsRequired();
                entity.Property(task => task.EindTijd)
                    .IsRequired();
            });
        }
    }
}
