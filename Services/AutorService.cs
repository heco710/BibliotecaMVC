using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services.Interfaces;

namespace BibliotecaMVC.Services;

public sealed class AutorService(IRepositorioAutor repositorio) : IAutorService
{
    public IReadOnlyList<Autor> ObtenerTodos() => repositorio.ObtenerTodos().OrderBy(item => item.Apellido).ToList();
    public Autor? ObtenerPorId(int id) => repositorio.ObtenerPorId(id);
    public bool Eliminar(int id) => repositorio.Eliminar(id);

    public ResultadoOperacion Agregar(Autor autor)
    {
        var errores = Validar(autor);
        if (errores.Count > 0) return new(true, errores);
        autor.ID = repositorio.Agregar(autor);
        return new(true, []);
    }

    public ResultadoOperacion Actualizar(Autor autor)
    {
        var errores = Validar(autor);
        if (errores.Count > 0) return new(true, errores);
        return new(repositorio.Actualizar(autor), []);
    }

    private static List<ValidationResult> Validar(Autor autor)
    {
        var errores = ResultadoOperacion.Validar(autor);
        if (autor.FechaNacimiento.Date > DateTime.Today)
            errores.Add(new("La fecha de nacimiento no puede estar en el futuro.", [nameof(Autor.FechaNacimiento)]));
        return errores;
    }
}
