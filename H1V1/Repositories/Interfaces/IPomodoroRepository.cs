using H1V1.Models.Entities;

namespace H1V1.Repositories.Interfaces
{
    public interface IPomodoroRepository
    {
        Task LogSessionAsync(PomodoroSession session);
        Task<int> GetTotalFocusMinutesAsync(int studentId);
    }
}
