using BibliotecaMVC.Models;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

public class LibrosController(ILibroService servicio) : Controller
{
    public IActionResult Index() => View(servicio.ObtenerTodos());

    public IActionResult Details(int id)
    {
        var libro = servicio.ObtenerPorId(id);
        return libro is null ? NotFound() : View(libro);
    }

    public IActionResult Create() => View(new Libro { AnioPublicacion = DateTime.Today.Year, Disponible = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Libro libro)
    {
        if (!ModelState.IsValid) return View(libro);
        var resultado = servicio.Agregar(libro);
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(libro);
        TempData["Mensaje"] = "El libro se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var libro = servicio.ObtenerPorId(id);
        return libro is null ? NotFound() : View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit([FromRoute] int id, Libro libro)
    {
        if (id != libro.ID) return BadRequest();
        if (!ModelState.IsValid) return View(libro);
        var resultado = servicio.Actualizar(libro);
        if (!resultado.Encontrado) return NotFound();
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(libro);
        TempData["Mensaje"] = "La información del libro se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var libro = servicio.ObtenerPorId(id);
        return libro is null ? NotFound() : View(libro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed([FromRoute] int id)
    {
        if (!servicio.Eliminar(id)) return NotFound();
        TempData["Mensaje"] = "El libro se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
