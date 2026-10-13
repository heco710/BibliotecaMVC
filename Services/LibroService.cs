using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services.Interfaces;

namespace BibliotecaMVC.Services;

public sealed class LibroService(IRepositorioLibro repositorio, IImagenService imagenes) : ILibroService
{
    public static readonly string[] Portadas = ["cien-anos-soledad.png", "casa-espiritus.png", "ficciones.png", "senor-presidente.png"];

    public Task<IReadOnlyList<Libro>> ObtenerTodosAsync() => repositorio.ObtenerTodosAsync();
    public Task<Libro?> ObtenerPorIdAsync(int id) => repositorio.ObtenerPorIdAsync(id);

    public async Task<bool> EliminarAsync(int id)
    {
        var imagen = (await repositorio.ObtenerPorIdAsync(id))?.Imagen;
        if (imagen is null || !await repositorio.EliminarAsync(id)) return false;
        imagenes.Eliminar(imagen);
        return true;
    }

    public async Task<ResultadoOperacion> AgregarAsync(Libro libro, IFormFile? archivo = null)
    {
        libro.ID = 0;
        var errores = await ValidarAsync(libro, archivo, imagenActual: null);
        if (errores.Count > 0) return new(true, errores);
        await GuardarConImagenAsync(libro, archivo, async () => { libro.ID = await repositorio.AgregarAsync(libro); return true; });
        return new(true, []);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Libro libro, IFormFile? archivo = null)
    {
        var imagenActual = (await repositorio.ObtenerPorIdAsync(libro.ID))?.Imagen;
        if (imagenActual is null) return new(false, []);
        var errores = await ValidarAsync(libro, archivo, imagenActual);
        if (errores.Count > 0) return new(true, errores);
        if (!await GuardarConImagenAsync(libro, archivo, () => repositorio.ActualizarAsync(libro))) return new(false, []);
        if (libro.Imagen != imagenActual) imagenes.Eliminar(imagenActual);
        return new(true, []);
    }

    // Guarda primero el archivo y, si la escritura en la base no se completa, lo borra para no dejar huérfanos.
    private async Task<bool> GuardarConImagenAsync(Libro libro, IFormFile? archivo, Func<Task<bool>> persistir)
    {
        if (archivo is not null) libro.Imagen = await imagenes.GuardarAsync(archivo);
        var guardado = false;
        try { return guardado = await persistir(); }
        finally { if (!guardado && archivo is not null) imagenes.Eliminar(libro.Imagen); }
    }

    private async Task<List<ValidationResult>> ValidarAsync(Libro libro, IFormFile? archivo, string? imagenActual)
    {
        var errores = ResultadoOperacion.Validar(libro);
        if (libro.AnioPublicacion > DateTime.Today.Year)
            errores.Add(new("El año de publicación no puede estar en el futuro.", [nameof(Libro.AnioPublicacion)]));
        if (archivo is not null)
        {
            if (imagenes.Validar(archivo) is string error) errores.Add(new(error, ["imagenArchivo"]));
        }
        // Sin archivo nuevo, solo se aceptan portadas del catálogo o la imagen que el libro ya tenía.
        else if (!Portadas.Contains(libro.Imagen) && libro.Imagen != imagenActual)
            errores.Add(new("Seleccione una imagen válida del catálogo.", [nameof(Libro.Imagen)]));
        if (errores.Count == 0 && await repositorio.ExisteIsbnAsync(libro.ISBN, libro.ID))
            errores.Add(new("Ya existe un libro con este ISBN.", [nameof(Libro.ISBN)]));
        return errores;
    }
}
