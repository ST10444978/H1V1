using H1V1.Models.Entities;

namespace H1V1.Repositories.Interfaces
{
    public interface IAcademicRepository
    {
        Task<List<Assessment>> GetPendingAssessmentsAsync(int studentId);
        Task<Assessment> AddAssessmentAsync(Assessment assessment);
        Task<bool> MarkAssessmentCompleteAsync(int assessmentId);
        Task<StudyPlan> SaveStudyPlanAsync(StudyPlan plan);
        Task<List<Resource>> GetResourcesByTopicAsync(string topic);
    }
}
