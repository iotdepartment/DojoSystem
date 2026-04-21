using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    [Authorize]
    [NoCache]
    public class EntrenadoresController : Controller
    {
        private readonly AppDbContext _context;
        public EntrenadoresController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var entrenadores = _context.Entrenadores.ToList();
            return View(entrenadores);
        }
    }
}
