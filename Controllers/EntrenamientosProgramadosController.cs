using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

            // NUEVO: Enviamos la lista de objetos completos para extraer Descripción y Límite en la vista
            ViewBag.EntrenamientosCompletos = _context.Entrenamientos.ToList();

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

        // GET: EntrenamientosProgramados/BuscarPorNumero/10452
        [HttpGet]
        public IActionResult BuscarPorNumero(int numeroEmpleado)
        {
            var emp = _context.Empleados.FirstOrDefault(e => e.NumeroEmpleado == numeroEmpleado);
            if (emp == null)
            {
                return NotFound();
            }

            // Retornamos el perfil completo necesario para la programación individual
            return Json(new
            {
                id = emp.ID,
                nombre = emp.NombreEmpleado,
                numero = emp.NumeroEmpleado,
                areaId = emp.AreaID ?? 0,
                turnoId = emp.TurnoID ?? 0
            });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPorArea(int areaId)
        {
            var empleados = await _context.Empleados
                .Where(e => e.AreaID == areaId && e.Activo == 1)
                .OrderBy(e => e.NumeroEmpleado)
                .Select(e => new
                {
                    id = e.ID,
                    numeroEmpleado = e.NumeroEmpleado,
                    nombre = e.NombreEmpleado,
                    areaId = e.AreaID,   // Necesario para mapear en JavaScript
                    turnoId = e.TurnoID  // Necesario para mapear en JavaScript
                })
                .ToListAsync();

            return Json(empleados);
        }
        // POST: EntrenamientosProgramados/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(EntrenamientosProgramados nuevaProgramacion, List<int> EmpleadosIds, List<int> AreasIds, List<int> TurnosIds)
        {
            // Verificamos que el entrenamiento sea válido y que se haya seleccionado al menos un empleado
            if (nuevaProgramacion.EntrenamientoID > 0 && EmpleadosIds != null && EmpleadosIds.Any())
            {
                // VALIDACIÓN DE SEGURIDAD: Las 3 listas deben estar perfectamente sincronizadas en tamaño
                if (EmpleadosIds.Count != AreasIds?.Count || EmpleadosIds.Count != TurnosIds?.Count)
                {
                    ModelState.AddModelError("", "Los datos de los empleados, áreas y turnos no coinciden.");
                    return RedirectToAction(nameof(Index));
                }

                // 1. Extraemos la fecha del formulario a una variable local limpia (sin horas)
                DateTime fechaFiltro = nuevaProgramacion.FechaProgramacion?.Date ?? DateTime.Today;

                // 2. Obtener la información de los empleados de golpe (incluimos NombreEmpleado para la alerta)
                var empleadosInfo = _context.Empleados
                    .Where(e => EmpleadosIds.Contains(e.ID))
                    .Select(e => new { e.ID, e.SupervisorID, e.NombreEmpleado, e.NumeroEmpleado })
                    .ToList();

                // 3. Consulta de duplicados en la base de datos
                var programacionesExistentes = _context.EntrenamientosProgramados
                    .Where(ep => ep.EntrenamientoID == nuevaProgramacion.EntrenamientoID
                              && ep.FechaProgramacion.HasValue
                              && ep.FechaProgramacion.Value.Date == fechaFiltro
                              && ep.EmpleadoID.HasValue
                              && EmpleadosIds.Contains(ep.EmpleadoID.Value))
                    .Select(ep => ep.EmpleadoID.Value)
                    .ToHashSet();

                bool seAgregaronNuevos = false;
                List<string> empleadosRepetidos = new List<string>(); // Lista para guardar los nombres de los duplicados

                for (int i = 0; i < EmpleadosIds.Count; i++)
                {
                    int empId = EmpleadosIds[i];
                    var empData = empleadosInfo.FirstOrDefault(e => e.ID == empId);

                    // VALIDACIÓN: Si está repetido, guardamos su nombre para el mensaje y saltamos el registro
                    if (programacionesExistentes.Contains(empId))
                    {
                        if (empData != null)
                        {
                            empleadosRepetidos.Add($"#{empData.NumeroEmpleado} - {empData.NombreEmpleado}");
                        }
                        continue;
                    }

                    int areaIdForm = AreasIds[i];
                    int turnoIdForm = TurnosIds[i];

                    var registroIndividual = new EntrenamientosProgramados
                    {
                        EntrenamientoID = nuevaProgramacion.EntrenamientoID,
                        EmpleadoID = empId,
                        FechaProgramacion = nuevaProgramacion.FechaProgramacion,
                        Status = 1, // 1 = Programado
                        SupervisorID = empData?.SupervisorID ?? 0,
                        AreaID = areaIdForm > 0 ? areaIdForm : (int?)null,
                        TurnoID = turnoIdForm > 0 ? turnoIdForm : (int?)null
                    };

                    _context.EntrenamientosProgramados.Add(registroIndividual);
                    seAgregaronNuevos = true;
                }

                // Guardar cambios si hay nuevos registros
                if (seAgregaronNuevos)
                {
                    await _context.SaveChangesAsync();
                    TempData["MensajeExito"] = "La programación del grupo se generó exitosamente.";
                }

                // Si hubo empleados repetidos, generamos la lista detallada para la vista
                if (empleadosRepetidos.Any())
                {
                    TempData["ListaRepetidos"] = empleadosRepetidos;
                    TempData["MensajeAdvertencia"] = "Los siguientes empleados no fueron agregados porque ya tienen este entrenamiento programado para esta fecha:";
                }

                return RedirectToAction(nameof(Index));
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

        [HttpPost]
        public async Task<IActionResult> EliminarGrupo(string idsString)
        {
            if (string.IsNullOrEmpty(idsString))
            {
                return BadRequest("No se proporcionaron identificadores válidos.");
            }

            try
            {
                // Convertimos el string "12,13,14" en una lista de enteros List<int> { 12, 13, 14 }
                var idsAGrabar = idsString.Split(',')
                                          .Select(int.Parse)
                                          .ToList();

                // Buscamos todos los registros que coincidan de golpe
                var registrosAEliminar = _context.EntrenamientosProgramados
                    .Where(ep => idsAGrabar.Contains(ep.ID))
                    .ToList();

                if (registrosAEliminar.Any())
                {
                    _context.EntrenamientosProgramados.RemoveRange(registrosAEliminar);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno al procesar la eliminación masiva.");
            }
        }

    }
}
