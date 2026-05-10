using Microsoft.EntityFrameworkCore;
using SmartStudyPlanner.Api.Models;

namespace SmartStudyPlanner.Api.Data
{
    public class StudyPlannerDbContext(DbContextOptions<StudyPlannerDbContext> options) : DbContext(options)
    {
        public DbSet<UserAccount> Users => Set<UserAccount>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<DeadlineItem> Deadlines => Set<DeadlineItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(user => user.Id);
                entity.HasIndex(user => user.Email).IsUnique();
                entity.Property(user => user.Naam)
                    .IsRequired()
                    .HasMaxLength(80);
                entity.Property(user => user.Email)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(user => user.WachtwoordHash)
                    .IsRequired()
                    .HasMaxLength(512);
                entity.Property(user => user.WachtwoordSalt)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(user => user.Rol)
                    .IsRequired()
                    .HasMaxLength(30);
                entity.Property(user => user.IsActief)
                    .HasDefaultValue(true);
            });

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
                entity.Property(task => task.Prioriteit)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(task => task.GeschatteStudietijdMinuten)
                    .IsRequired();
                entity.Property(task => task.Status)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(task => task.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<DeadlineItem>(entity =>
            {
                entity.ToTable("Deadlines");
                entity.HasKey(deadline => deadline.Id);
                entity.Property(deadline => deadline.Titel)
                    .IsRequired()
                    .HasMaxLength(120);
                entity.Property(deadline => deadline.Beschrijving)
                    .HasMaxLength(500);
                entity.Property(deadline => deadline.Datum)
                    .IsRequired();
                entity.Property(deadline => deadline.EindTijd)
                    .IsRequired();
                entity.Property(deadline => deadline.Prioriteit)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(deadline => deadline.UserId)
                    .IsRequired();
            });
        }
    }
}
