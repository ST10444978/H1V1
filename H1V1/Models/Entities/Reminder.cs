namespace H1V1.Models.Entities
{
    public class ReminderDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DateTimeDisplay { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }

    public class CreateReminderRequest
    {
        public string Title { get; set; } = string.Empty;
        public string DateTimeDisplay { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
    }
}
