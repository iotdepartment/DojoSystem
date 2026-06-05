using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmpleadosController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Empleados
        public IActionResult Index()
        {
            var empleados = _context.Empleados.Include(e => e.Area).ToList();
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

            foreach (var u in empleados)
            {
                u.Foto = "/Images/default.png";
                string[] extensiones = { ".png", ".jpg", ".jpeg" };

                foreach (var ext in extensiones)
                {
                    string filePath = Path.Combine(uploadsFolder, $"{u.NumeroEmpleado}{ext}");
                    if (System.IO.File.Exists(filePath))
                    {
                        u.Foto = $"/Images/{u.NumeroEmpleado}{ext}";
                        break;
                    }
                }
            }

            // 1. Cargar Áreas
            ViewBag.ListaAreas = _context.Areas
                .Select(a => new SelectListItem { Value = a.ID.ToString(), Text = a.Nombre })
                .ToList();

            // 2. Cargar Turnos
            ViewBag.ListaTurnos = _context.Turnos
                .Select(t => new SelectListItem { Value = t.ID.ToString(), Text = t.NombreTurno })
                .ToList();

            // 3. NUEVO: Cargar Supervisores cruzando con la tabla Empleados para obtener el Nombre
            var querySupervisores = (from s in _context.Supervisores
                                     join e in _context.Empleados on s.EmpleadoID equals e.ID
                                     select new SelectListItem
                                     {
                                         Value = s.ID.ToString(),
                                         Text = e.NombreEmpleado
                                     }).ToList();

            // Insertamos la opción NA en la posición 0 con el valor "0"
            querySupervisores.Insert(0, new SelectListItem { Value = "0", Text = "NA" });

            ViewBag.ListaSupervisores = querySupervisores;


            return View(empleados);
        }


        // POST: Empleados/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Empleados nuevoEmpleado)
        {
            if (ModelState.IsValid)
            {
                // 1. Guardar primero el registro en la Base de Datos para asegurar la consistencia
                _context.Add(nuevoEmpleado);
                await _context.SaveChangesAsync();

                // 2. Procesar la subida del archivo si se seleccionó uno
                if (nuevoEmpleado.FotoFile != null && nuevoEmpleado.FotoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

                    // Asegurar que el directorio de imágenes exista en wwwroot
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Obtener la extensión del archivo original (.jpg, .png, etc.)
                    string extension = Path.GetExtension(nuevoEmpleado.FotoFile.FileName).ToLower();

                    // El nombre físico del archivo será el NumeroEmpleado asignado
                    string fileName = $"{nuevoEmpleado.NumeroEmpleado}{extension}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    // Guardar el archivo en el almacenamiento del servidor de forma asíncrona
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await nuevoEmpleado.FotoFile.CopyToAsync(fileStream);
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // Si falla la validación, recargamos la vista con errores
            return RedirectToAction(nameof(Index));
        }

        // GET: Empleados/ObtenerEmpleado/5
        [HttpGet]
        public IActionResult ObtenerEmpleado(int id)
        {
            var empleado = _context.Empleados.Find(id);
            if (empleado == null)
            {
                return NotFound();
            }
            // Retornamos el objeto plano para que JavaScript lo lea fácilmente
            return Json(empleado);
        }

        // POST: Empleados/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Empleados empleadoModificado)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleadoModificado);
                    await _context.SaveChangesAsync();

                    // Procesar nueva imagen si el usuario subió una
                    if (empleadoModificado.FotoFile != null && empleadoModificado.FotoFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

                        // Borrar posibles imágenes anteriores con otras extensiones para no acumular basura
                        string[] extensiones = { ".png", ".jpg", ".jpeg" };
                        foreach (var ext in extensiones)
                        {
                            string oldFile = Path.Combine(uploadsFolder, $"{empleadoModificado.NumeroEmpleado}{ext}");
                            if (System.IO.File.Exists(oldFile))
                            {
                                System.IO.File.Delete(oldFile);
                            }
                        }

                        // Guardar el nuevo archivo
                        string extension = Path.GetExtension(empleadoModificado.FotoFile.FileName).ToLower();
                        string filePath = Path.Combine(uploadsFolder, $"{empleadoModificado.NumeroEmpleado}{extension}");

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await empleadoModificado.FotoFile.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Empleados.Any(e => e.ID == empleadoModificado.ID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Empleados/Eliminar/5
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }

            // 1. Borrar las imágenes físicas asociadas en wwwroot
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
            string[] extensiones = { ".png", ".jpg", ".jpeg" };

            foreach (var ext in extensiones)
            {
                string filePath = Path.Combine(uploadsFolder, $"{empleado.NumeroEmpleado}{ext}");
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // 2. Eliminar el registro de la base de datos
            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();

            // Retornamos una respuesta de éxito HTTP 200 sin redirigir desde C#
            return Ok();
        }

    }
}
