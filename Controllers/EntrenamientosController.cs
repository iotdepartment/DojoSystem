using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    public class EntrenamientosController : Controller
    {
        private readonly AppDbContext _context;

        public EntrenamientosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Entrenamientos
        public IActionResult Index()
        {
            // 1. Obtener la lista limpia de entrenamientos desde la base de datos
            var listaEntrenamientos = _context.Entrenamientos.ToList();

            // 2. Cargar catálogo de Áreas mapeado a SelectListItem (sirve para el modal y las tarjetas)
            ViewBag.ListaAreas = _context.Areas
                .Select(a => new SelectListItem { Value = a.ID.ToString(), Text = a.Nombre })
                .ToList();

            // 3. Cargar catálogo de Entrenadores (unido con Empleados para obtener el nombre real)
            ViewBag.ListaEntrenadores = (from ent in _context.Entrenadores
                                         join e in _context.Empleados on ent.EmpleadoID equals e.ID
                                         select new SelectListItem
                                         {
                                             Value = ent.ID.ToString(),
                                             Text = e.NombreEmpleado
                                         }).ToList();

            // Envia el modelo puro de la base de datos
            return View(listaEntrenamientos);
        }

        // POST: Entrenamientos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Entrenamientos nuevoEntrenamiento)
        {
            if (ModelState.IsValid)
            {
                _context.Entrenamientos.Add(nuevoEntrenamiento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Entrenamientos/ObtenerEntrenamiento/5
        [HttpGet]
        public IActionResult ObtenerEntrenamiento(int id)
        {
            var entrenamiento = _context.Entrenamientos.Find(id);
            if (entrenamiento == null)
            {
                return NotFound();
            }
            return Json(entrenamiento);
        }

        // POST: Entrenamientos/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Entrenamientos entrenamientoModificado)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entrenamientoModificado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Entrenamientos.Any(e => e.Id == entrenamientoModificado.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Entrenamientos/Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            // 1. Buscar el registro del entrenamiento en la base de datos por su ID
            var entrenamiento = await _context.Entrenamientos.FindAsync(id);

            // 2. Si el registro no existe, retornar un error HTTP 404 para avisar a JavaScript
            if (entrenamiento == null)
            {
                return NotFound();
            }

            // 3. Remover el registro del contexto de datos
            _context.Entrenamientos.Remove(entrenamiento);

            // 4. Confirmar y guardar permanentemente los cambios en SQL Server
            await _context.SaveChangesAsync();

            // 5. Retornar un estado HTTP 200 (Éxito) sin redireccionar la página desde C#
            return Ok();
        }


    }
}
