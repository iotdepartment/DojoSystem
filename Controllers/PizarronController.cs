using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        // GET: Pizarron/DashboardAsistencia
        public IActionResult Index(int? areaId, int? turnoId)
        {
            // 1. Cargar catálogos para los selectores de filtrado
            ViewBag.ListaAreas = _context.Areas
                .Select(a => new SelectListItem { Value = a.ID.ToString(), Text = a.Nombre, Selected = a.ID == areaId })
                .ToList();

            ViewBag.ListaTurnos = _context.Turnos
                .Select(t => new SelectListItem { Value = t.ID.ToString(), Text = t.NombreTurno, Selected = t.ID == turnoId })
                .ToList();

            // 2. Consulta base de programaciones
            var query = _context.EntrenamientosProgramados.AsQueryable();

            // 3. Aplicar filtros dinámicos basados en el perfil del Empleado
            if (areaId.HasValue && areaId > 0)
            {
                query = query.Where(p => p.AreaID == areaId);
            }
            if (turnoId.HasValue && turnoId > 0)
            {
                query = query.Where(p => p.TurnoID == turnoId);
            }

            var registrosFiltrados = query.ToList();

            // 4. Traer todos los entrenamientos para cruzarlos con las estadísticas
            var entrenamientos = _context.Entrenamientos.ToList();

            // 5. Construir las métricas calculadas
            var estadisticasCursos = entrenamientos.Select(e =>
            {
                var registrosDelCurso = registrosFiltrados.Where(p => p.EntrenamientoID == e.Id).ToList();

                int convocados = registrosDelCurso.Count;
                int asistieron = registrosDelCurso.Count(p => p.Status == 2 || p.FechaAsistencia.HasValue);

                double porcentaje = convocados > 0 ? Math.Round((double)asistieron / convocados * 100, 1) : 0;

                return new
                {
                    CursoId = e.Id,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    Limite = e.Limite ?? 0,
                    Convocados = convocados,
                    Asistieron = asistieron,
                    PorcentajeAsistencia = porcentaje
                };
            }).Where(x => x.Convocados > 0).ToList();

            ViewBag.AreaSeleccionada = areaId;
            ViewBag.TurnoSeleccionado = turnoId;

            return View(estadisticasCursos);
        }

    }
}
