using H1V1.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace H1V1.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Assessment> Assessments => Set<Assessment>();
        public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
        public DbSet<Resource> Resources => Set<Resource>();
        public DbSet<PomodoroSession> PomodoroSessions => Set<PomodoroSession>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().HasData(
                new Student { Id = 1, Name = "Demo Student", Major = "Computer Science" }
            );
            modelBuilder.Entity<Resource>().HasData(
                new Resource { Id = 1, CourseOrTopic = "Data Structures", Title = "Data Structures Basics", Url = "https://example.com/ds", ResourceType = "Article" },
                new Resource { Id = 2, CourseOrTopic = "Calculus", Title = "Calculus Overview", Url = "https://example.com/calc", ResourceType = "Video" }
            );
        }
    }
}
