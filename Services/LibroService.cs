using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services.Interfaces;

namespace BibliotecaMVC.Services;

public sealed class LibroService(IRepositorioLibro repositorio) : ILibroService
{
    public IReadOnlyList<Libro> ObtenerTodos() => repositorio.ObtenerTodos().OrderBy(item => item.Titulo).ToList();
    public Libro? ObtenerPorId(int id) => repositorio.ObtenerPorId(id);
    public bool Eliminar(int id) => repositorio.Eliminar(id);

    public ResultadoOperacion Agregar(Libro libro)
    {
        var errores = Validar(libro);
        if (errores.Count > 0) return new(true, errores);
        libro.ID = repositorio.Agregar(libro);
        return new(true, []);
    }

    public ResultadoOperacion Actualizar(Libro libro)
    {
        var errores = Validar(libro);
        if (errores.Count > 0) return new(true, errores);
        return new(repositorio.Actualizar(libro), []);
    }

    private static List<ValidationResult> Validar(Libro libro)
    {
        var errores = ResultadoOperacion.Validar(libro);
        if (libro.AnioPublicacion > DateTime.Today.Year)
            errores.Add(new("El año de publicación no puede estar en el futuro.", [nameof(Libro.AnioPublicacion)]));
        string[] imagenesPermitidas = ["cien-anos-soledad.png", "casa-espiritus.png", "ficciones.png", "senor-presidente.png"];
        if (!imagenesPermitidas.Contains(libro.Imagen))
            errores.Add(new("Seleccione una imagen válida del catálogo.", [nameof(Libro.Imagen)]));
        return errores;
    }
}
