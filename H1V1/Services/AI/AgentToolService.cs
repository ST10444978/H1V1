using H1V1.Models.Entities;
using H1V1.Repositories.Interfaces;
using System.ComponentModel;

namespace H1V1.Services.AI
{
    public class AgentToolService
    {
        private readonly IAcademicRepository _repo;

        public AgentToolService(IAcademicRepository repo)
        {
            _repo = repo;
        }

        [Description("Retrieves all upcoming, incomplete assessments and exams for a student.")]
        public async Task<string> GetPendingAssessments(int studentId)
        {
            var list = await _repo.GetPendingAssessmentsAsync(studentId);
            if (!list.Any()) return "No upcoming assessments found.";
            return string.Join("\n", list.Select(a => $"- ID: {a.Id}, Title: {a.Title}, Course: {a.CourseCode}, Due: {a.DueDate:yyyy-MM-dd}"));
        }

        [Description("Adds a new exam, assignment, or project to track for a student.")]
        public async Task<string> TrackAssessment(int studentId, string title, string courseCode, DateTime dueDate)
        {
            var created = await _repo.AddAssessmentAsync(new Assessment
            {
                StudentId = studentId,
                Title = title,
                CourseCode = courseCode,
                DueDate = dueDate,
                IsCompleted = false
            });
            return $"Successfully tracked assessment '{created.Title}' (ID: {created.Id}) due on {created.DueDate:yyyy-MM-dd}.";
        }

        [Description("Saves a generated study plan to the database for future reference.")]
        public async Task<string> SaveStudyPlan(int studentId, string title, string planMarkdown)
        {
            await _repo.SaveStudyPlanAsync(new StudyPlan
            {
                StudentId = studentId,
                Title = title,
                GeneratedPlanMarkdown = planMarkdown
            });
            return "Study plan saved successfully to student record.";
        }

        [Description("Finds educational materials, articles, and video links for a subject.")]
        public async Task<string> SearchResources(string topic)
        {
            var items = await _repo.GetResourcesByTopicAsync(topic);
            if (!items.Any()) return $"No curated resources found for '{topic}'.";
            return string.Join("\n", items.Select(r => $"- [{r.ResourceType}] {r.Title}: {r.Url}"));
        }
    }
}
