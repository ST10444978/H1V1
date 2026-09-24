namespace H1V1.Models.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public List<Assessment> Assessments { get; set; } = new();
        public List<StudyPlan> StudyPlans { get; set; } = new();
    }
}
