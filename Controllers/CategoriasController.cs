using BibliotecaMVC.Models;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

// Consultar: cualquier visitante. Agregar, editar y eliminar: Bibliotecario o Admin.
// Logging con el mismo criterio que LibrosController: Information por operación, Warning ante registros
// inexistentes o datos rechazados; las excepciones se registran con LogError en ErrorDeDatosFilter.
[Authorize]
public class CategoriasController(ICategoriaService servicio, ILogger<CategoriasController> logger) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("Consulta del listado de categorías solicitada por {Usuario}.", User.Identity?.Name ?? "un visitante anónimo");
        var categorias = await servicio.ObtenerTodosAsync();
        logger.LogInformation("Listado de categorías obtenido: {Cantidad} registros.", categorias.Count);
        return View(categorias);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        logger.LogInformation("Consulta del detalle de la categoría {CategoriaId}.", id);
        var categoria = await servicio.ObtenerPorIdAsync(id);
        if (categoria is null)
        {
            logger.LogWarning("Se solicitó el detalle de la categoría {CategoriaId}, que no existe.", id);
            return NotFound();
        }
        return View(categoria);
    }

    [Authorize(Roles = Roles.Gestores)]
    public IActionResult Create()
    {
        logger.LogInformation("Formulario de creación de categoría abierto por {Usuario}.", User.Identity?.Name);
        return View(new Categoria());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        logger.LogInformation("Inicio de creación de la categoría \"{Nombre}\" por {Usuario}.", categoria.Nombre, User.Identity?.Name);
        if (!ModelState.IsValid) return Rechazada(categoria, "creación", ModelState.ErrorCount);
        var resultado = await servicio.AgregarAsync(categoria);
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return Rechazada(categoria, "creación", resultado.Errores.Count);
        logger.LogInformation("Categoría \"{Nombre}\" creada correctamente.", categoria.Nombre);
        TempData["Mensaje"] = "La categoría se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> Edit(int id)
    {
        logger.LogInformation("Formulario de edición de la categoría {CategoriaId} abierto por {Usuario}.", id, User.Identity?.Name);
        var categoria = await servicio.ObtenerPorIdAsync(id);
        if (categoria is null)
        {
            logger.LogWarning("Se intentó editar la categoría {CategoriaId}, que no existe.", id);
            return NotFound();
        }
        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> Edit([FromRoute] int id, Categoria categoria)
    {
        logger.LogInformation("Inicio de edición de la categoría {CategoriaId} por {Usuario}.", id, User.Identity?.Name);
        if (id != categoria.ID)
        {
            logger.LogWarning("Edición rechazada: el identificador de la ruta ({RutaId}) no coincide con el del formulario ({FormularioId}).", id, categoria.ID);
            return BadRequest();
        }
        if (!ModelState.IsValid) return Rechazada(categoria, "edición", ModelState.ErrorCount);
        var resultado = await servicio.ActualizarAsync(categoria);
        if (!resultado.Encontrado)
        {
            logger.LogWarning("Se intentó guardar la categoría {CategoriaId}, que ya no existe.", id);
            return NotFound();
        }
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return Rechazada(categoria, "edición", resultado.Errores.Count);
        logger.LogInformation("Categoría {CategoriaId} actualizada correctamente.", id);
        TempData["Mensaje"] = "La categoría se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> Delete(int id)
    {
        logger.LogInformation("Confirmación de eliminación de la categoría {CategoriaId} solicitada por {Usuario}.", id, User.Identity?.Name);
        var categoria = await servicio.ObtenerPorIdAsync(id);
        if (categoria is null)
        {
            logger.LogWarning("Se intentó eliminar la categoría {CategoriaId}, que no existe.", id);
            return NotFound();
        }
        return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Gestores)]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
    {
        logger.LogInformation("Inicio de eliminación de la categoría {CategoriaId} por {Usuario}.", id, User.Identity?.Name);
        if (!await servicio.EliminarAsync(id))
        {
            logger.LogWarning("No se eliminó la categoría {CategoriaId} porque no existe.", id);
            return NotFound();
        }
        logger.LogInformation("Categoría {CategoriaId} eliminada correctamente.", id);
        TempData["Mensaje"] = "La categoría se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private ViewResult Rechazada(Categoria categoria, string operacion, int errores)
    {
        logger.LogWarning("La {Operacion} de la categoría \"{Nombre}\" fue rechazada por {Errores} errores de validación.", operacion, categoria.Nombre, errores);
        return View(categoria);
    }
}
