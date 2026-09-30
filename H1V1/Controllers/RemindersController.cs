using Microsoft.AspNetCore.Mvc;
using H1V1.DTOs;


namespace H1V1.Controllers
{
    

    public class RemindersController : Controller
    {
        private static readonly List<ReminderDto> _inMemoryReminders = new()
        {
            new ReminderDto { Id = 1, Title = "Database Assignment", DateTimeDisplay = "Monday, 29 Sep · 09:00 AM", Priority = "High", IsDone = false },
            new ReminderDto { Id = 2, Title = "C# Test", DateTimeDisplay = "Friday, 2 Oct · 09:00 AM", Priority = "High", IsDone = false },
            new ReminderDto { Id = 3, Title = "Study Plan Session", DateTimeDisplay = "Sunday · 4 Oct · 10:00 AM", Priority = "Medium", IsDone = false }
        };

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("api/reminders")]
        public IActionResult GetRemindersApi()
        {
            lock (_inMemoryReminders)
            {
                return Ok(_inMemoryReminders.Where(r => !r.IsDone).ToList());
            }
        }

        [HttpPost("api/reminders")]
        public IActionResult CreateReminder([FromBody] CreateReminderRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { message = "Title is required." });
            }

            lock (_inMemoryReminders)
            {
                var newReminder = new ReminderDto
                {
                    Id = _inMemoryReminders.Count > 0 ? _inMemoryReminders.Max(r => r.Id) + 1 : 1,
                    Title = request.Title,
                    DateTimeDisplay = string.IsNullOrWhiteSpace(request.DateTimeDisplay) ? DateTime.Now.ToString("dddd, d MMM · hh:mm tt") : request.DateTimeDisplay,
                    Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority,
                    IsDone = false
                };
                _inMemoryReminders.Add(newReminder);
                return Ok(newReminder);
            }
        }

        [HttpPost("api/reminders/{id}/done")]
        public IActionResult MarkDone(int id)
        {
            lock (_inMemoryReminders)
            {
                var reminder = _inMemoryReminders.FirstOrDefault(r => r.Id == id);
                if (reminder == null) return NotFound();
                reminder.IsDone = true;
                return Ok(new { success = true });
            }
        }

        [HttpPost("api/reminders/{id}/snooze")]
        public IActionResult Snooze(int id)
        {
            lock (_inMemoryReminders)
            {
                var reminder = _inMemoryReminders.FirstOrDefault(r => r.Id == id);
                if (reminder == null) return NotFound();
                reminder.DateTimeDisplay += " (Snoozed +15m)";
                return Ok(reminder);
            }
        }
    }
}
