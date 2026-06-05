using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    public class SupervisoresController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SupervisoresController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Supervisores
        public IActionResult Index()
        {
            // 1. Cruzamos la tabla Supervisores con Empleados, Áreas y Turnos
            var listaSupervisores = (from s in _context.Supervisores
                                     join e in _context.Empleados on s.EmpleadoID equals e.ID

                                     // Unimos con Áreas
                                     join a in _context.Areas on e.AreaID equals a.ID into areaJoin
                                     from area in areaJoin.DefaultIfEmpty()

                                         // Unimos con Turnos
                                     join t in _context.Turnos on e.TurnoID equals t.ID into turnoJoin
                                     from turno in turnoJoin.DefaultIfEmpty()

                                     select new SupervisorViewModel
                                     {
                                         SupervisorID = s.ID,
                                         EmpleadoID = e.ID,
                                         NombreEmpleado = e.NombreEmpleado,
                                         NumeroEmpleado = e.NumeroEmpleado,
                                         AreaNombre = area != null ? area.Nombre : "Sin Área",
                                         TurnoNombre = turno != null ? turno.NombreTurno : "Sin Turno",
                                         SupervisorPadreID = e.SupervisorID // El supervisor del supervisor
                                     }).ToList();

            // 2. Traemos la lista general de supervisores en memoria para pintar sus nombres (si tienen)
            var todosLosSupervisores = (from s in _context.Supervisores
                                        join e in _context.Empleados on s.EmpleadoID equals e.ID
                                        select new { s.ID, e.NombreEmpleado }).ToList();

            // 3. Resolvemos las imágenes desde wwwroot y el nombre de su supervisor
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
            string[] extensiones = { ".png", ".jpg", ".jpeg" };

            foreach (var s in listaSupervisores)
            {
                // Resolver Foto
                s.Foto = "/Images/default.png";
                foreach (var ext in extensiones)
                {
                    string filePath = Path.Combine(uploadsFolder, $"{s.NumeroEmpleado}{ext}");
                    if (System.IO.File.Exists(filePath))
                    {
                        s.Foto = $"/Images/{s.NumeroEmpleado}{ext}";
                        break;
                    }
                }

                // Resolver el nombre de su propio jefe/supervisor (si aplica)
                if (s.SupervisorPadreID != null && s.SupervisorPadreID > 0)
                {
                    var jefe = todosLosSupervisores.FirstOrDefault(x => x.ID == s.SupervisorPadreID);
                    s.SupervisorNombre = jefe != null ? jefe.NombreEmpleado : "NA";
                }
                else
                {
                    s.SupervisorNombre = "NA";
                }
            }

            // 4. Cargar empleados que aún no son supervisores para el modal de asignación
            var yaSonSupervisores = _context.Supervisores.Select(s => s.EmpleadoID).ToList();

            ViewBag.ListaEmpleadosDisponibles = _context.Empleados
                .Where(e => !yaSonSupervisores.Contains(e.ID))
                .Select(e => new SelectListItem
                {
                    Value = e.ID.ToString(),
                    Text = $"[{e.NumeroEmpleado}] - {e.NombreEmpleado}"
                }).ToList();

            return View(listaSupervisores);
        }

        // POST: Supervisores/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int EmpleadoID)
        {
            if (EmpleadoID > 0)
            {
                var nuevoSupervisor = new Supervisores { EmpleadoID = EmpleadoID };
                _context.Supervisores.Add(nuevoSupervisor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Supervisores/Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var supervisor = await _context.Supervisores.FindAsync(id);
            if (supervisor != null)
            {
                _context.Supervisores.Remove(supervisor);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}
