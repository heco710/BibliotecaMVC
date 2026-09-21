using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services.Interfaces;

public interface IAutorService
{
    Task<IReadOnlyList<Autor>> ObtenerTodosAsync();
    Task<Autor?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion> AgregarAsync(Autor autor);
    Task<ResultadoOperacion> ActualizarAsync(Autor autor);
    Task<bool> EliminarAsync(int id);
}
