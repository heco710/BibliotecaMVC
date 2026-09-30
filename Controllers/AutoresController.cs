using BibliotecaMVC.Models;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

[Authorize]
public class AutoresController(IAutorService servicio) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index() => View(await servicio.ObtenerTodosAsync());

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var autor = await servicio.ObtenerPorIdAsync(id);
        return autor is null ? NotFound() : View(autor);
    }

    public IActionResult Create() => View(new Autor { FechaNacimiento = DateTime.Today, Activo = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Autor autor)
    {
        if (!ModelState.IsValid) return View(autor);
        var resultado = await servicio.AgregarAsync(autor);
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(autor);
        TempData["Mensaje"] = "El autor se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var autor = await servicio.ObtenerPorIdAsync(id);
        return autor is null ? NotFound() : View(autor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, Autor autor)
    {
        if (id != autor.ID) return BadRequest();
        if (!ModelState.IsValid) return View(autor);
        var resultado = await servicio.ActualizarAsync(autor);
        if (!resultado.Encontrado) return NotFound();
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(autor);
        TempData["Mensaje"] = "La información del autor se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var autor = await servicio.ObtenerPorIdAsync(id);
        return autor is null ? NotFound() : View(autor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
    {
        if (!await servicio.EliminarAsync(id)) return NotFound();
        TempData["Mensaje"] = "El autor se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
