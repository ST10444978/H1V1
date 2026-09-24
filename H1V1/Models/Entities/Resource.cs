namespace H1V1.Models.Entities
{
    public class Resource
    {
        public int Id { get; set; }
        public string CourseOrTopic { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
    }
}
