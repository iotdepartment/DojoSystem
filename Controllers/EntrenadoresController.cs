using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    public class EntrenadoresController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EntrenadoresController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Entrenadores
        public IActionResult Index()
        {
            // 1. Cargar el listado de supervisores global en memoria para resolver los nombres de los jefes
            var listaSupervisoresGlobal = (from s in _context.Supervisores
                                           join e in _context.Empleados on s.EmpleadoID equals e.ID
                                           select new { s.ID, e.NombreEmpleado }).ToList();

            // 2. Consulta principal cruzando Entrenadores con Empleados, Áreas y Turnos
            var listaEntrenadores = (from ent in _context.Entrenadores
                                     join e in _context.Empleados on ent.EmpleadoID equals e.ID

                                     join a in _context.Areas on e.AreaID equals a.ID into areaJoin
                                     from area in areaJoin.DefaultIfEmpty()

                                     join t in _context.Turnos on e.TurnoID equals t.ID into turnoJoin
                                     from turno in turnoJoin.DefaultIfEmpty()

                                     select new EntrenadorViewModel
                                     {
                                         EntrenadorID = ent.ID,
                                         EmpleadoID = e.ID,
                                         NombreEmpleado = e.NombreEmpleado,
                                         NumeroEmpleado = e.NumeroEmpleado,
                                         AreaNombre = area != null ? area.Nombre : "Sin Área",
                                         TurnoNombre = turno != null ? turno.NombreTurno : "Sin Turno",
                                         Foto = "/Images/default.png" // Por defecto
                                     }).ToList();

            // 3. Resolver imágenes físicas desde wwwroot y nombres de supervisores
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
            string[] extensiones = { ".png", ".jpg", ".jpeg" };

            foreach (var ent in listaEntrenadores)
            {
                // Buscar el objeto Empleado original para validar su SupervisorID actual
                var empOriginal = _context.Empleados.FirstOrDefault(x => x.ID == ent.EmpleadoID);
                if (empOriginal?.SupervisorID != null && empOriginal.SupervisorID > 0)
                {
                    var jefe = listaSupervisoresGlobal.FirstOrDefault(x => x.ID == empOriginal.SupervisorID);
                    ent.SupervisorNombre = jefe != null ? jefe.NombreEmpleado : "NA";
                }
                else
                {
                    ent.SupervisorNombre = "NA";
                }

                // Resolver Foto de perfil física
                foreach (var ext in extensiones)
                {
                    string filePath = Path.Combine(uploadsFolder, $"{ent.NumeroEmpleado}{ext}");
                    if (System.IO.File.Exists(filePath))
                    {
                        ent.Foto = $"/Images/{ent.NumeroEmpleado}{ext}";
                        break;
                    }
                }
            }

            // 4. Filtrar empleados que AÚN NO SON entrenadores para el select del modal
            var yaSonEntrenadores = _context.Entrenadores.Select(x => x.EmpleadoID).ToList();

            ViewBag.ListaEmpleadosDisponibles = _context.Empleados
                .Where(e => !yaSonEntrenadores.Contains(e.ID))
                .Select(e => new SelectListItem
                {
                    Value = e.ID.ToString(),
                    Text = $"[{e.NumeroEmpleado}] - {e.NombreEmpleado}"
                }).ToList();

            return View(listaEntrenadores);
        }

        // POST: Entrenadores/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int EmpleadoID)
        {
            if (EmpleadoID > 0)
            {
                var nuevoEntrenador = new Entrenadores { EmpleadoID = EmpleadoID };
                _context.Entrenadores.Add(nuevoEntrenador);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Entrenadores/Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var entrenador = await _context.Entrenadores.FindAsync(id);
            if (entrenador != null)
            {
                _context.Entrenadores.Remove(entrenador);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}
