using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories;

public interface IRepositorioCategoria
{
    Task<IReadOnlyList<Categoria>> ObtenerTodosAsync();
    Task<Categoria?> ObtenerPorIdAsync(int id);
    Task<int> AgregarAsync(Categoria categoria);
    Task<bool> ActualizarAsync(Categoria categoria);
    Task<bool> EliminarAsync(int id);
}
