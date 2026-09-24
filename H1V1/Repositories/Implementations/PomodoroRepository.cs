using H1V1.Data;
using H1V1.Models.Entities;
using H1V1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace H1V1.Repositories.Implementations
{
    // Ensure interface inheritance is explicitly declared
    public class PomodoroRepository : IPomodoroRepository
    {
        private readonly AppDbContext _db;
        public PomodoroRepository(AppDbContext db) => _db = db;

        public async Task LogSessionAsync(PomodoroSession session)
        {
            _db.PomodoroSessions.Add(session);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetTotalFocusMinutesAsync(int studentId)
        {
            return await _db.PomodoroSessions
                .Where(p => p.StudentId == studentId)
                .SumAsync(p => p.DurationMinutes);
        }
    }
}