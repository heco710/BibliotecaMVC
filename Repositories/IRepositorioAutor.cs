using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories;

public interface IRepositorioAutor
{
    Task<IReadOnlyList<Autor>> ObtenerTodosAsync();
    Task<Autor?> ObtenerPorIdAsync(int id);
    Task<int> AgregarAsync(Autor autor);
    Task<bool> ActualizarAsync(Autor autor);
    Task<bool> EliminarAsync(int id);
}
