using BibliotecaMVC.Models;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

public class AutoresController(IAutorService servicio) : Controller
{
    public IActionResult Index() => View(servicio.ObtenerTodos());

    public IActionResult Details(int id)
    {
        var autor = servicio.ObtenerPorId(id);
        return autor is null ? NotFound() : View(autor);
    }

    public IActionResult Create() => View(new Autor { FechaNacimiento = DateTime.Today, Activo = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Autor autor)
    {
        if (!ModelState.IsValid) return View(autor);
        var resultado = servicio.Agregar(autor);
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(autor);
        TempData["Mensaje"] = "El autor se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var autor = servicio.ObtenerPorId(id);
        return autor is null ? NotFound() : View(autor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit([FromRoute] int id, Autor autor)
    {
        if (id != autor.ID) return BadRequest();
        if (!ModelState.IsValid) return View(autor);
        var resultado = servicio.Actualizar(autor);
        if (!resultado.Encontrado) return NotFound();
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(autor);
        TempData["Mensaje"] = "La información del autor se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var autor = servicio.ObtenerPorId(id);
        return autor is null ? NotFound() : View(autor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed([FromRoute] int id)
    {
        if (!servicio.Eliminar(id)) return NotFound();
        TempData["Mensaje"] = "El autor se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
