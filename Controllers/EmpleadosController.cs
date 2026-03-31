using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly AppDbContext _context;
        public EmpleadosController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.Areas = new SelectList(_context.Areas, "ID", "Area");
            var empleados = _context.Empleados
                .Include(e => e.Area)
                .ToList();

            return View(empleados);
        }



        [HttpPost]
        public IActionResult Delete(int id)
        {
            var Empleado = _context.Empleados.Find(id);
            if (Empleado == null)
            {
                return NotFound();
            }

            _context.Empleados.Remove(Empleado);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Empleados empleado)
        {
            if (ModelState.IsValid)
            {
                _context.Empleados.Add(empleado);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Areas = new SelectList(_context.Areas, "ID", "Area", empleado.AreaID);
            var empleados = _context.Empleados.Include(e => e.Area).ToList();
            return View("Index", empleados);
        }

    }
}
