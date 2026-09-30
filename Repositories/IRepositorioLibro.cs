using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories;

public interface IRepositorioLibro
{
    Task<IReadOnlyList<Libro>> ObtenerTodosAsync();
    Task<Libro?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteIsbnAsync(string isbn, int excluirId);
    Task<int> AgregarAsync(Libro libro);
    Task<bool> ActualizarAsync(Libro libro);
    Task<bool> EliminarAsync(int id);
}
