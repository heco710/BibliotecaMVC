using BibliotecaMVC.Models;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

public class LibrosController(ILibroService servicio) : Controller
{
    public async Task<IActionResult> Index() => View(await servicio.ObtenerTodosAsync());

    public async Task<IActionResult> Details(int id)
    {
        var libro = await servicio.ObtenerPorIdAsync(id);
        return libro is null ? NotFound() : View(libro);
    }

    public IActionResult Create() => View(new Libro { AnioPublicacion = DateTime.Today.Year, Disponible = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Libro libro)
    {
        if (!ModelState.IsValid) return View(libro);
        var resultado = await servicio.AgregarAsync(libro);
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(libro);
        TempData["Mensaje"] = "El libro se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var libro = await servicio.ObtenerPorIdAsync(id);
        return libro is null ? NotFound() : View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, Libro libro)
    {
        if (id != libro.ID) return BadRequest();
        if (!ModelState.IsValid) return View(libro);
        var resultado = await servicio.ActualizarAsync(libro);
        if (!resultado.Encontrado) return NotFound();
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(libro);
        TempData["Mensaje"] = "La información del libro se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var libro = await servicio.ObtenerPorIdAsync(id);
        return libro is null ? NotFound() : View(libro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
    {
        if (!await servicio.EliminarAsync(id)) return NotFound();
        TempData["Mensaje"] = "El libro se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
