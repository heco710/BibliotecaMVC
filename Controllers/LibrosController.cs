using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

public class LibrosController : Controller
{
    private static readonly object SyncRoot = new();

    private static readonly List<Libro> Libros =
    [
        new() { ID = 1, Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Realismo mágico", AnioPublicacion = 1967, ISBN = "978-0307474728", Descripcion = "La historia de la familia Buendía y del inolvidable pueblo de Macondo.", Imagen = "cien-anos-soledad.png", Disponible = true },
        new() { ID = 2, Titulo = "La casa de los espíritus", Autor = "Isabel Allende", Categoria = "Narrativa", AnioPublicacion = 1982, ISBN = "978-0525433477", Descripcion = "Una saga familiar atravesada por la memoria, el amor y los cambios históricos.", Imagen = "casa-espiritus.png", Disponible = true },
        new() { ID = 3, Titulo = "Ficciones", Autor = "Jorge Luis Borges", Categoria = "Cuentos", AnioPublicacion = 1944, ISBN = "978-0802130303", Descripcion = "Laberintos, bibliotecas y mundos posibles en una colección esencial.", Imagen = "ficciones.png", Disponible = false },
        new() { ID = 4, Titulo = "El señor Presidente", Autor = "Miguel Ángel Asturias", Categoria = "Novela", AnioPublicacion = 1946, ISBN = "978-8420674209", Descripcion = "Una obra fundamental de la literatura guatemalteca sobre el poder y sus sombras.", Imagen = "senor-presidente.png", Disponible = true }
    ];

    public IActionResult Index()
    {
        lock (SyncRoot)
        {
            return View(Libros.OrderBy(libro => libro.Titulo).ToList());
        }
    }

    public IActionResult Details(int id)
    {
        var libro = BuscarLibro(id);
        return libro is null ? NotFound() : View(libro);
    }

    public IActionResult Create()
    {
        return View(new Libro { AnioPublicacion = DateTime.Today.Year, Disponible = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Libro libro)
    {
        ValidarLibro(libro);
        if (!ModelState.IsValid)
        {
            return View(libro);
        }

        lock (SyncRoot)
        {
            libro.ID = Libros.Count == 0 ? 1 : Libros.Max(item => item.ID) + 1;
            Libros.Add(libro);
        }

        TempData["Mensaje"] = "El libro se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var libro = BuscarLibro(id);
        return libro is null ? NotFound() : View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Libro libro)
    {
        if (id != libro.ID)
        {
            return BadRequest();
        }

        ValidarLibro(libro);
        if (!ModelState.IsValid)
        {
            return View(libro);
        }

        lock (SyncRoot)
        {
            var index = Libros.FindIndex(item => item.ID == id);
            if (index < 0)
            {
                return NotFound();
            }

            Libros[index] = libro;
        }

        TempData["Mensaje"] = "La información del libro se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var libro = BuscarLibro(id);
        return libro is null ? NotFound() : View(libro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        lock (SyncRoot)
        {
            var libro = Libros.FirstOrDefault(item => item.ID == id);
            if (libro is null)
            {
                return NotFound();
            }

            Libros.Remove(libro);
        }

        TempData["Mensaje"] = "El libro se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private static Libro? BuscarLibro(int id)
    {
        lock (SyncRoot)
        {
            return Libros.FirstOrDefault(item => item.ID == id);
        }
    }

    private void ValidarLibro(Libro libro)
    {
        if (libro.AnioPublicacion > DateTime.Today.Year)
        {
            ModelState.AddModelError(nameof(Libro.AnioPublicacion), "El año de publicación no puede estar en el futuro.");
        }

        string[] imagenesPermitidas = ["cien-anos-soledad.png", "casa-espiritus.png", "ficciones.png", "senor-presidente.png"];
        if (!imagenesPermitidas.Contains(libro.Imagen))
        {
            ModelState.AddModelError(nameof(Libro.Imagen), "Seleccione una imagen válida del catálogo.");
        }
    }
}
