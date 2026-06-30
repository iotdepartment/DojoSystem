using Microsoft.AspNetCore.Mvc;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    public class PizarronController : Controller
    {
        private readonly AppDbContext _context;
        public PizarronController(AppDbContext context)
        {
            _context = context;
        }
        // GET: Pizarron/Index
        public IActionResult Index()
        {
            try
            {
                var areasConDatos = _context.EntrenamientosProgramados
                    .Where(p => p.AreaID.HasValue)
                    .Select(p => p.AreaID.Value)
                    .Distinct()
                    .ToList();

                var areas = _context.Areas
                    .Where(a => areasConDatos.Contains(a.ID))
                    .OrderBy(a => a.Nombre)
                    .ToList();

                return View(areas ?? new List<TrainingsDashboard.Models.Areas>());
            }
            catch (Exception)
            {
                return View(new List<TrainingsDashboard.Models.Areas>());
            }
        }

        // GET: Pizarron/ObtenerMetricasPorArea
        [HttpGet]
        public IActionResult ObtenerMetricasPorArea(int areaId, int turnoId)
        {
            try
            {
                // 1. Filtramos las programaciones usando los campos nativos de tu modelo EntrenamientosProgramados
                var registrosFiltrados = (from ep in _context.EntrenamientosProgramados
                                          join emp in _context.Empleados on ep.EmpleadoID equals emp.ID
                                          where ep.AreaID == areaId && ep.TurnoID == turnoId
                                          select new
                                          {
                                              ep.ID,
                                              ep.EntrenamientoID,
                                              ep.Status,
                                              NumeroEmpleado = emp.NumeroEmpleado,
                                              NombreEmpleado = emp.NombreEmpleado
                                          }).ToList();

                var entrenamientos = _context.Entrenamientos.ToList();

                // 2. Construimos las estadísticas separando de forma estricta por las reglas de tu campo Status
                var estadisticas = entrenamientos.Select(e =>
                {
                    var registrosDelCurso = registrosFiltrados
                        .Where(r => r.EntrenamientoID == e.Id)
                        .ToList();

                    // Convocados totales = Pendientes (1) + Asistieron (2)
                    int convocados = registrosDelCurso.Count(r => r.Status == 1 || r.Status == 2);

                    // REGLA DE NEGOCIO: Status == 2 significa que ya asistieron
                    int asistieron = registrosDelCurso.Count(r => r.Status == 2);

                    double porcentaje = convocados > 0 ? Math.Round((double)asistieron / convocados * 100, 1) : 0;

                    // REGLA DE NEGOCIO: Status == 1 significa que están pendientes
                    var pendientes = registrosDelCurso
                        .Where(r => r.Status == 1)
                        .Select(r => new
                        {
                            num = r.NumeroEmpleado ?? 0,
                            nombre = r.NombreEmpleado ?? "Sin Nombre"
                        })
                        .OrderBy(r => r.num)
                        .ToList();

                    return new
                    {
                        id = e.Id,
                        nombre = e.Nombre,
                        descripcion = e.Descripcion ?? "Sin descripción registrada.",
                        convocados = convocados,
                        asistieron = asistieron,
                        porcentajeAsistencia = porcentaje,
                        empleadosPendientes = pendientes // Arreglo que lee el script de JavaScript
                    };
                }).Where(x => x.convocados > 0).ToList();

                return Json(estadisticas);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerMetricasPorArea: " + ex.Message);
                return BadRequest(new List<object>());
            }
        }





    }
}
