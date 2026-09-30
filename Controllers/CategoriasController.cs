using BibliotecaMVC.Models;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

[Authorize]
public class CategoriasController(ICategoriaService servicio) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index() => View(await servicio.ObtenerTodosAsync());

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var categoria = await servicio.ObtenerPorIdAsync(id);
        return categoria is null ? NotFound() : View(categoria);
    }

    public IActionResult Create() => View(new Categoria());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (!ModelState.IsValid) return View(categoria);
        var resultado = await servicio.AgregarAsync(categoria);
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(categoria);
        TempData["Mensaje"] = "La categoría se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await servicio.ObtenerPorIdAsync(id);
        return categoria is null ? NotFound() : View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, Categoria categoria)
    {
        if (id != categoria.ID) return BadRequest();
        if (!ModelState.IsValid) return View(categoria);
        var resultado = await servicio.ActualizarAsync(categoria);
        if (!resultado.Encontrado) return NotFound();
        ModelState.AgregarErrores(resultado.Errores);
        if (!resultado.Exitoso) return View(categoria);
        TempData["Mensaje"] = "La categoría se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await servicio.ObtenerPorIdAsync(id);
        return categoria is null ? NotFound() : View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
    {
        if (!await servicio.EliminarAsync(id)) return NotFound();
        TempData["Mensaje"] = "La categoría se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
