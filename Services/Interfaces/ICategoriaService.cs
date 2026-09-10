using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services.Interfaces;

public interface ICategoriaService
{
    Task<IReadOnlyList<Categoria>> ObtenerTodosAsync();
    Task<Categoria?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion> AgregarAsync(Categoria categoria);
    Task<ResultadoOperacion> ActualizarAsync(Categoria categoria);
    Task<bool> EliminarAsync(int id);
}
