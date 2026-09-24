using Microsoft.AspNetCore.Mvc;

namespace H1V1.Controllers
{
    public class StudyPlanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}