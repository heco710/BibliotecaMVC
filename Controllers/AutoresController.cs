using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

public class AutoresController : Controller
{
    private static readonly object SyncRoot = new();

    private static readonly List<Autor> Autores =
    [
        new() { ID = 1, Nombre = "Gabriel", Apellido = "García Márquez", Nacionalidad = "Colombiana", FechaNacimiento = new DateTime(1927, 3, 6), Activo = false },
        new() { ID = 2, Nombre = "Isabel", Apellido = "Allende", Nacionalidad = "Chilena", FechaNacimiento = new DateTime(1942, 8, 2), Activo = true },
        new() { ID = 3, Nombre = "Jorge Luis", Apellido = "Borges", Nacionalidad = "Argentina", FechaNacimiento = new DateTime(1899, 8, 24), Activo = false },
        new() { ID = 4, Nombre = "Laura", Apellido = "Esquivel", Nacionalidad = "Mexicana", FechaNacimiento = new DateTime(1950, 9, 30), Activo = true },
        new() { ID = 5, Nombre = "Miguel Ángel", Apellido = "Asturias", Nacionalidad = "Guatemalteca", FechaNacimiento = new DateTime(1899, 10, 19), Activo = false }
    ];

    public IActionResult Index()
    {
        lock (SyncRoot)
        {
            return View(Autores.OrderBy(autor => autor.Apellido).ToList());
        }
    }

    public IActionResult Details(int id)
    {
        var autor = BuscarAutor(id);
        return autor is null ? NotFound() : View(autor);
    }

    public IActionResult Create()
    {
        return View(new Autor { FechaNacimiento = DateTime.Today, Activo = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Autor autor)
    {
        ValidarFecha(autor.FechaNacimiento);
        if (!ModelState.IsValid)
        {
            return View(autor);
        }

        lock (SyncRoot)
        {
            autor.ID = Autores.Count == 0 ? 1 : Autores.Max(item => item.ID) + 1;
            Autores.Add(autor);
        }

        TempData["Mensaje"] = "El autor se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var autor = BuscarAutor(id);
        return autor is null ? NotFound() : View(autor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Autor autor)
    {
        if (id != autor.ID)
        {
            return BadRequest();
        }

        ValidarFecha(autor.FechaNacimiento);
        if (!ModelState.IsValid)
        {
            return View(autor);
        }

        lock (SyncRoot)
        {
            var index = Autores.FindIndex(item => item.ID == id);
            if (index < 0)
            {
                return NotFound();
            }

            Autores[index] = autor;
        }

        TempData["Mensaje"] = "La información del autor se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var autor = BuscarAutor(id);
        return autor is null ? NotFound() : View(autor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        lock (SyncRoot)
        {
            var autor = Autores.FirstOrDefault(item => item.ID == id);
            if (autor is null)
            {
                return NotFound();
            }

            Autores.Remove(autor);
        }

        TempData["Mensaje"] = "El autor se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private static Autor? BuscarAutor(int id)
    {
        lock (SyncRoot)
        {
            return Autores.FirstOrDefault(item => item.ID == id);
        }
    }

    private void ValidarFecha(DateTime fechaNacimiento)
    {
        if (fechaNacimiento.Date > DateTime.Today)
        {
            ModelState.AddModelError(nameof(Autor.FechaNacimiento), "La fecha de nacimiento no puede estar en el futuro.");
        }
    }
}
