using BibliotecaMVC.Models;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

// Autenticación: quién es el usuario. Autorización: qué puede hacer según su rol.
// Consultar: Usuario, Bibliotecario o Admin. Agregar, editar y eliminar: Bibliotecario o Admin.
// Logging: Information al iniciar y completar cada operación, Warning si el registro no existe o los datos
// son rechazados. Las excepciones de datos las registra con LogError ErrorDeDatosFilter, y las demás el manejador global.
[Authorize(Roles = Roles.Todos)]
public class LibrosController(ILibroService servicio, ILogger<LibrosController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("Consulta del listado de libros solicitada por {Usuario}.", User.Identity?.Name);
        var libros = await servicio.ObtenerTodosAsync();
        logger.LogInformation("Listado de libros obtenido: {Cantidad} registros.", libros.Count);
        return View(libros);
    }

    public async Task<IActionResult> Details(int id)
    {
        logger.LogInformation("Consulta del detalle del libro {LibroId}.", id);
        var libro = await servicio.ObtenerPorIdAsync(id);
        if (libro is null)
        {
            logger.LogWarning("Se solicitó el detalle del libro {LibroId}, que no existe.", id);
            return NotFound();
        }
        return View(libro);
    }

    [Authorize(Roles = Roles.Gestores)]
    public IActionResult Create()
    {
        logger.LogInformation("Formulario de creación de libro abierto por {Usuario}.", User.Identity?.Name);
        return View(new Libro { AnioPublicacion = DateTime.Today.Year, Disponible = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> Create(Libro libro, IFormFile? imagenArchivo)
    {
        logger.LogInformation("Inicio de creación del libro \"{Titulo}\" por {Usuario}.", libro.Titulo, User.Identity?.Name);
        if (!ModelState.IsValid) return Rechazado(libro, "creación", ModelState.ErrorCount);
        var resultado = await servicio.AgregarAsync(libro, imagenArchivo);
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return Rechazado(libro, "creación", resultado.Errores.Count);
        logger.LogInformation("Libro {LibroId} \"{Titulo}\" creado correctamente.", libro.ID, libro.Titulo);
        TempData["Mensaje"] = "El libro se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> Edit(int id)
    {
        logger.LogInformation("Formulario de edición del libro {LibroId} abierto por {Usuario}.", id, User.Identity?.Name);
        var libro = await servicio.ObtenerPorIdAsync(id);
        if (libro is null)
        {
            logger.LogWarning("Se intentó editar el libro {LibroId}, que no existe.", id);
            return NotFound();
        }
        return View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> Edit([FromRoute] int id, Libro libro, IFormFile? imagenArchivo)
    {
        logger.LogInformation("Inicio de edición del libro {LibroId} por {Usuario}.", id, User.Identity?.Name);
        if (id != libro.ID)
        {
            logger.LogWarning("Edición rechazada: el identificador de la ruta ({RutaId}) no coincide con el del formulario ({FormularioId}).", id, libro.ID);
            return BadRequest();
        }
        if (!ModelState.IsValid) return Rechazado(libro, "edición", ModelState.ErrorCount);
        var resultado = await servicio.ActualizarAsync(libro, imagenArchivo);
        if (!resultado.Encontrado)
        {
            logger.LogWarning("Se intentó guardar el libro {LibroId}, que ya no existe.", id);
            return NotFound();
        }
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return Rechazado(libro, "edición", resultado.Errores.Count);
        logger.LogInformation("Libro {LibroId} actualizado correctamente.", id);
        TempData["Mensaje"] = "La información del libro se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> Delete(int id)
    {
        logger.LogInformation("Confirmación de eliminación del libro {LibroId} solicitada por {Usuario}.", id, User.Identity?.Name);
        var libro = await servicio.ObtenerPorIdAsync(id);
        if (libro is null)
        {
            logger.LogWarning("Se intentó eliminar el libro {LibroId}, que no existe.", id);
            return NotFound();
        }
        return View(libro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
    {
        logger.LogInformation("Inicio de eliminación del libro {LibroId} por {Usuario}.", id, User.Identity?.Name);
        if (!await servicio.EliminarAsync(id))
        {
            logger.LogWarning("No se eliminó el libro {LibroId} porque no existe.", id);
            return NotFound();
        }
        logger.LogInformation("Libro {LibroId} eliminado correctamente.", id);
        TempData["Mensaje"] = "El libro se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private ViewResult Rechazado(Libro libro, string operacion, int errores)
    {
        logger.LogWarning("La {Operacion} del libro \"{Titulo}\" fue rechazada por {Errores} errores de validación.", operacion, libro.Titulo, errores);
        return View(libro);
    }
}
