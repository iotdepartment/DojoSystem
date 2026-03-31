using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    [Authorize]
    [NoCache]
    public class AreasController : Controller
    {
        private readonly AppDbContext _context;

        public AreasController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var areas = _context.Areas.ToList();
            return View(areas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TrainingsDashboard.Models.Areas area)
        {
            if (ModelState.IsValid)
            {
                _context.Areas.Add(area);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View("Index", _context.Areas.ToList());
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var area = _context.Areas.Find(id);
            if (area == null)
            {
                return NotFound();
            }

            _context.Areas.Remove(area);
            _context.SaveChanges();

            return Ok();
        }
    }
}