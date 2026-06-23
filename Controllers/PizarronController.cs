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
                // 1. Filtramos estrictamente por el Área y el Turno solicitados por la rotación de JavaScript
                var registrosFiltrados = _context.EntrenamientosProgramados
                    .Where(p => p.AreaID == areaId && p.TurnoID == turnoId)
                    .ToList();

                var entrenamientos = _context.Entrenamientos.ToList();

                // 2. Construimos las estadísticas asegurando la separación estricta de turnos
                var estadisticas = entrenamientos.Select(e =>
                {
                    // CORRECCIÓN CRÍTICA: Filtramos por EntrenamientoID Y por TurnoID en los registros ya en memoria
                    var registrosDelCurso = registrosFiltrados
                        .Where(p => p.EntrenamientoID == e.Id && p.TurnoID == turnoId)
                        .ToList();

                    int convocados = registrosDelCurso.Count;
                    int asistieron = registrosDelCurso.Count(p => p.Status == 2 || p.FechaAsistencia.HasValue);
                    double porcentaje = convocados > 0 ? Math.Round((double)asistieron / convocados * 100, 1) : 0;

                    return new
                    {
                        nombre = e.Nombre,
                        descripcion = e.Descripcion ?? "Sin descripción registrada.",
                        convocados = convocados,
                        asistieron = asistieron,
                        porcentajeAsistencia = porcentaje
                    };
                }).Where(x => x.convocados > 0).ToList(); // Solo incluimos cursos que tengan personal convocado en este turno específico

                return Json(estadisticas);
            }
            catch (Exception)
            {
                return BadRequest(new List<object>());
            }
        }




    }
}
