using H1V1.Data;
using H1V1.Models.Entities;
using H1V1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore; 

namespace H1V1.Repositories.Implementations
{
    public class AcademicRepository : IAcademicRepository
    {
        private readonly AppDbContext _db;
        public AcademicRepository(AppDbContext db) => _db = db;

        public async Task<List<Assessment>> GetPendingAssessmentsAsync(int studentId) =>
            await _db.Assessments
                .Where(a => a.StudentId == studentId && !a.IsCompleted)
                .OrderBy(a => a.DueDate)
                .ToListAsync();

        public async Task<Assessment> AddAssessmentAsync(Assessment assessment)
        {
            _db.Assessments.Add(assessment);
            await _db.SaveChangesAsync();
            return assessment;
        }

        public async Task<bool> MarkAssessmentCompleteAsync(int assessmentId)
        {
            var item = await _db.Assessments.FindAsync(assessmentId);
            if (item == null) return false;
            item.IsCompleted = true;
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<StudyPlan> SaveStudyPlanAsync(StudyPlan plan)
        {
            _db.StudyPlans.Add(plan);
            await _db.SaveChangesAsync();
            return plan;
        }

        public async Task<List<Resource>> GetResourcesByTopicAsync(string topic) =>
            await _db.Resources
                .Where(r => r.CourseOrTopic.Contains(topic))
                .ToListAsync();
    }
}