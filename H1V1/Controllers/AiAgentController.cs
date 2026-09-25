using Microsoft.AspNetCore.Mvc;

namespace H1V1.Controllers
{
    public class AiAgentController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Chat(string message)
        {
            // Connect this to your existing AI service
            // later.

            return RedirectToAction("Index");
        }
    }
}
