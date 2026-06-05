using Microsoft.AspNetCore.Mvc;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    public class TurnosController : Controller
    {
        private readonly AppDbContext _context;

        public TurnosController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var turnos = _context.Turnos.ToList();
            return View(turnos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TrainingsDashboard.Models.Turnos turno)
        {
            if (ModelState.IsValid)
            {
                _context.Turnos.Add(turno);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View("Index", _context.Turnos.ToList());
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var turno = _context.Turnos.Find(id);
            if (turno == null)
            {
                return NotFound();
            }

            _context.Turnos.Remove(turno);
            _context.SaveChanges();

            return Ok();
        }
    }
}
