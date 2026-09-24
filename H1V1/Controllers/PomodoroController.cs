using H1V1.Models.Entities;
using H1V1.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace H1V1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PomodoroController : ControllerBase
    {
        private readonly IPomodoroRepository _pomodoroRepo;

        public PomodoroController(IPomodoroRepository pomodoroRepo) => _pomodoroRepo = pomodoroRepo;

        [HttpPost("log")]
        public async Task<IActionResult> LogSession([FromBody] PomodoroSession session)
        {
            await _pomodoroRepo.LogSessionAsync(session);
            return Ok(new { success = true });
        }

        [HttpGet("stats/{studentId}")]
        public async Task<IActionResult> GetStats(int studentId)
        {
            var totalMinutes = await _pomodoroRepo.GetTotalFocusMinutesAsync(studentId);
            return Ok(new { totalFocusMinutes = totalMinutes });
        }
    }
}
