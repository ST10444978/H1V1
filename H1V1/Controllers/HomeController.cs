using H1V1.Data;
using H1V1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace H1V1.Controllers
{
    public class DashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public int AssessmentsDueCount { get; set; }
        public List<H1V1.Models.Entities.Assessment> UpcomingAssessments { get; set; } = new();
    }

    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            int studentId = 1;
            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            var upcomingAssessments = await _db.Assessments
                .Where(a => a.StudentId == studentId && !a.IsCompleted)
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                StudentName = student?.Name ?? "Student",
                AssessmentsDueCount = upcomingAssessments.Count,
                UpcomingAssessments = upcomingAssessments
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
