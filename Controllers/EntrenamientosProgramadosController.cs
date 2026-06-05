using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    // El nombre de la clase define la convención de la carpeta de vistas (Views/EntrenamientosProgramados/)
    public class EntrenamientosProgramadosController : Controller
    {
        private readonly AppDbContext _context;

        public EntrenamientosProgramadosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: EntrenamientosProgramados
        public IActionResult Index()
        {
            var programaciones = _context.EntrenamientosProgramados.ToList();

            ViewBag.ListaEntrenamientos = _context.Entrenamientos
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Nombre })
                .ToList();

            ViewBag.ListaEmpleados = _context.Empleados
                .Select(emp => new SelectListItem { Value = emp.ID.ToString(), Text = $"[{emp.NumeroEmpleado}] - {emp.NombreEmpleado}" })
                .ToList();

            ViewBag.ListaAreas = _context.Areas
                .Select(a => new SelectListItem { Value = a.ID.ToString(), Text = a.Nombre })
                .ToList();

            ViewBag.ListaTurnos = _context.Turnos
                .Select(t => new SelectListItem { Value = t.ID.ToString(), Text = t.NombreTurno })
                .ToList();

            ViewBag.ListaSupervisores = (from s in _context.Supervisores
                                         join e in _context.Empleados on s.EmpleadoID equals e.ID
                                         select new SelectListItem
                                         {
                                             Value = s.ID.ToString(),
                                             Text = e.NombreEmpleado
                                         }).ToList();

            return View(programaciones);
        }

        // GET: EntrenamientosProgramados/ObtenerSupervisorEmpleado/5
        [HttpGet]
        public IActionResult ObtenerSupervisorEmpleado(int empleadoId)
        {
            // Buscamos el empleado seleccionado
            var empleado = _context.Empleados.FirstOrDefault(e => e.ID == empleadoId);
            if (empleado == null) return NotFound();

            // Retornamos directamente su SupervisorID actual (si no tiene, devuelve 0)
            return Json(new { supervisorId = empleado.SupervisorID ?? 0 });
        }

        // POST: EntrenamientosProgramados/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(EntrenamientosProgramados nuevaProgramacion)
        {
            if (ModelState.IsValid)
            {
                // El campo nuevaProgramacion.FechaProgramacion ya viene lleno con el día que eligió el usuario
                nuevaProgramacion.Status = 1; // 1 = Programado

                _context.EntrenamientosProgramados.Add(nuevaProgramacion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        // GET: EntrenamientosProgramados/ObtenerProgramacion/5
        [HttpGet]
        public IActionResult ObtenerProgramacion(int id)
        {
            var prog = _context.EntrenamientosProgramados.Find(id);
            if (prog == null) return NotFound();
            return Json(prog);
        }

        // POST: EntrenamientosProgramados/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EntrenamientosProgramados progModificada)
        {
            if (ModelState.IsValid)
            {
                if (progModificada.Status == 2 && progModificada.FechaAsistencia == null)
                {
                    progModificada.FechaAsistencia = DateTime.Now;
                }

                _context.Update(progModificada);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: EntrenamientosProgramados/Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var prog = await _context.EntrenamientosProgramados.FindAsync(id);
            if (prog == null) return NotFound();

            _context.EntrenamientosProgramados.Remove(prog);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
