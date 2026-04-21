using Microsoft.AspNetCore.Mvc;

namespace TrainingsDashboard.Controllers
{
    public class TurnosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
