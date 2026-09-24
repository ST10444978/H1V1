namespace H1V1.Models.Entities
{
    public class PomodoroSession
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
