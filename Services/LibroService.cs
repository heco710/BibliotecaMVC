using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services.Interfaces;

namespace BibliotecaMVC.Services;

public sealed class LibroService(IRepositorioLibro repositorio) : ILibroService
{
    public Task<IReadOnlyList<Libro>> ObtenerTodosAsync() => repositorio.ObtenerTodosAsync();
    public Task<Libro?> ObtenerPorIdAsync(int id) => repositorio.ObtenerPorIdAsync(id);
    public Task<bool> EliminarAsync(int id) => repositorio.EliminarAsync(id);

    public async Task<ResultadoOperacion> AgregarAsync(Libro libro)
    {
        libro.ID = 0;
        var errores = await ValidarAsync(libro);
        if (errores.Count > 0) return new(true, errores);
        libro.ID = await repositorio.AgregarAsync(libro);
        return new(true, []);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Libro libro)
    {
        var errores = await ValidarAsync(libro);
        if (errores.Count > 0) return new(true, errores);
        return new(await repositorio.ActualizarAsync(libro), []);
    }

    private async Task<List<ValidationResult>> ValidarAsync(Libro libro)
    {
        var errores = ResultadoOperacion.Validar(libro);
        if (libro.AnioPublicacion > DateTime.Today.Year)
            errores.Add(new("El año de publicación no puede estar en el futuro.", [nameof(Libro.AnioPublicacion)]));
        string[] imagenesPermitidas = ["cien-anos-soledad.png", "casa-espiritus.png", "ficciones.png", "senor-presidente.png"];
        if (!imagenesPermitidas.Contains(libro.Imagen))
            errores.Add(new("Seleccione una imagen válida del catálogo.", [nameof(Libro.Imagen)]));
        if (errores.Count == 0 && await repositorio.ExisteIsbnAsync(libro.ISBN, libro.ID))
            errores.Add(new("Ya existe un libro con este ISBN.", [nameof(Libro.ISBN)]));
        return errores;
    }
}
