using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    public class AsistenciaController : Controller
    {
        private readonly AppDbContext _context;

        public AsistenciaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Asistencia
        public IActionResult Index(int? entrenamientoId, string fecha)
        {
            // 1. Cargar solo los entrenamientos que tienen alumnos pendientes (Status == 1)
            var pendientesIds = _context.EntrenamientosProgramados
                .Where(p => p.Status == 1)
                .Select(p => p.EntrenamientoID)
                .Distinct()
                .ToList();

            ViewBag.ListaEntrenamientos = _context.Entrenamientos
                .Where(e => pendientesIds.Contains(e.Id))
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Nombre, Selected = (e.Id == entrenamientoId) })
                .ToList();

            // 2. Cargar catálogos base en memoria para traducir los IDs de las filas
            ViewBag.ListaEmpleados = _context.Empleados.ToDictionary(e => e.ID, e => $"[{e.NumeroEmpleado}] - {e.NombreEmpleado}");
            ViewBag.ListaAreas = _context.Areas.ToDictionary(a => a.ID, a => a.Nombre);
            ViewBag.ListaTurnos = _context.Turnos.ToDictionary(t => t.ID, t => t.NombreTurno);

            // SOLUCIÓN: Inicializar siempre la lista de fechas como vacía para evitar errores NullReference en la vista
            ViewBag.ListaFechasDisponibles = new List<SelectListItem>();
            List<EntrenamientosProgramados> alumnosPendientes = new List<EntrenamientosProgramados>();

            // 3. Si el usuario ya filtró por un entrenamiento, gestionamos las fechas y los alumnos
            if (entrenamientoId.HasValue && entrenamientoId > 0)
            {
                ViewBag.EntrenamientoSeleccionado = entrenamientoId;

                // Recuperamos todas las fechas programadas disponibles para ese curso
                var fechasDisponibles = _context.EntrenamientosProgramados
                    .Where(p => p.EntrenamientoID == entrenamientoId && p.Status == 1 && p.FechaProgramacion.HasValue)
                    .Select(p => p.FechaProgramacion.Value.Date)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();

                // 4. Si también seleccionó una fecha, convertimos el string a DateTime y filtramos los alumnos
                if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out DateTime fechaFiltro))
                {
                    ViewBag.FechaSeleccionada = fecha;

                    alumnosPendientes = _context.EntrenamientosProgramados
                        .Where(p => p.EntrenamientoID == entrenamientoId
                                 && p.Status == 1
                                 && p.FechaProgramacion.HasValue
                                 && p.FechaProgramacion.Value.Date == fechaFiltro.Date)
                        .OrderBy(p => p.EmpleadoID)
                        .ToList();

                    // Mapeamos las fechas al select indicando cuál es la seleccionada
                    ViewBag.ListaFechasDisponibles = fechasDisponibles
                        .Select(d => new SelectListItem
                        {
                            Value = d.ToString("yyyy-MM-dd"),
                            Text = d.ToString("dd/MM/yyyy"),
                            Selected = (d.Date == fechaFiltro.Date)
                        }).ToList();
                }
                else
                {
                    // Si eligió curso pero aún no elige fecha, llenamos el select de fechas sin marcar ninguna
                    ViewBag.ListaFechasDisponibles = fechasDisponibles
                        .Select(d => new SelectListItem
                        {
                            Value = d.ToString("yyyy-MM-dd"),
                            Text = d.ToString("dd/MM/yyyy")
                        }).ToList();
                }
            }

            return View(alumnosPendientes);
        }



        // POST: Asistencia/GuardarAsistenciaMasiva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarAsistenciaMasiva(List<int> ProgramacionesIds, DateTime FechaAsistenciaReal)
        {
            if (ProgramacionesIds != null && ProgramacionesIds.Any())
            {
                // Si el usuario no seleccionó fecha en el input, tomamos el día de hoy
                DateTime fechaFinal = FechaAsistenciaReal == DateTime.MinValue ? DateTime.Now : FechaAsistenciaReal;

                // Traer los registros específicos de la base de datos
                var registrosAActualizar = _context.EntrenamientosProgramados
                    .Where(p => ProgramacionesIds.Contains(p.ID))
                    .ToList();

                foreach (var p in registrosAActualizar)
                {
                    p.Status = 2; // 2 = Asistió
                    p.FechaAsistencia = fechaFinal;
                    _context.Update(p);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { entrenamientoId = Request.Form["EntrenamientoFilterID"] });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFechasPorEntrenamiento(int entrenamientoId)
        {
            var fechas = await _context.EntrenamientosProgramados
                .Where(ep => ep.EntrenamientoID == entrenamientoId && ep.Status == 1 && ep.FechaProgramacion.HasValue)
                .Select(ep => ep.FechaProgramacion.Value.Date)
                .Distinct()
                .OrderByDescending(d => d) // Mostrar las más recientes primero
                .Select(d => new
                {
                    value = d.ToString("yyyy-MM-dd"),
                    text = d.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(fechas);
        }

    }
}
