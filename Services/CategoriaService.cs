using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services.Interfaces;

namespace BibliotecaMVC.Services;

public sealed class CategoriaService(IRepositorioCategoria repositorio) : ICategoriaService
{
    public Task<IReadOnlyList<Categoria>> ObtenerTodosAsync() => repositorio.ObtenerTodosAsync();
    public Task<Categoria?> ObtenerPorIdAsync(int id) => repositorio.ObtenerPorIdAsync(id);
    public Task<bool> EliminarAsync(int id) => repositorio.EliminarAsync(id);

    public async Task<ResultadoOperacion> AgregarAsync(Categoria categoria)
    {
        var errores = ResultadoOperacion.Validar(categoria);
        if (errores.Count > 0) return new(true, errores);
        Normalizar(categoria);
        categoria.ID = await repositorio.AgregarAsync(categoria);
        return new(true, []);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Categoria categoria)
    {
        var errores = ResultadoOperacion.Validar(categoria);
        if (errores.Count > 0) return new(true, errores);
        Normalizar(categoria);
        return new(await repositorio.ActualizarAsync(categoria), []);
    }

    private static void Normalizar(Categoria categoria)
    {
        categoria.Nombre = categoria.Nombre.Trim();
        categoria.Descripcion = string.IsNullOrWhiteSpace(categoria.Descripcion) ? null : categoria.Descripcion.Trim();
    }
}
