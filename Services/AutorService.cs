using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services.Interfaces;

namespace BibliotecaMVC.Services;

public sealed class AutorService(IRepositorioAutor repositorio) : IAutorService
{
    public Task<IReadOnlyList<Autor>> ObtenerTodosAsync() => repositorio.ObtenerTodosAsync();
    public Task<Autor?> ObtenerPorIdAsync(int id) => repositorio.ObtenerPorIdAsync(id);
    public Task<bool> EliminarAsync(int id) => repositorio.EliminarAsync(id);

    public async Task<ResultadoOperacion> AgregarAsync(Autor autor)
    {
        var errores = Validar(autor);
        if (errores.Count > 0) return new(true, errores);
        autor.ID = 0;
        autor.ID = await repositorio.AgregarAsync(autor);
        return new(true, []);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Autor autor)
    {
        var errores = Validar(autor);
        if (errores.Count > 0) return new(true, errores);
        return new(await repositorio.ActualizarAsync(autor), []);
    }

    private static List<ValidationResult> Validar(Autor autor)
    {
        var errores = ResultadoOperacion.Validar(autor);
        if (autor.FechaNacimiento.Date > DateTime.Today)
            errores.Add(new("La fecha de nacimiento no puede estar en el futuro.", [nameof(Autor.FechaNacimiento)]));
        return errores;
    }
}
