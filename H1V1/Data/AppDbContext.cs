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
        }
    }
}
