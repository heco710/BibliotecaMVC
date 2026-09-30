using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services.Interfaces;

public interface ILibroService
{
    Task<IReadOnlyList<Libro>> ObtenerTodosAsync();
    Task<Libro?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion> AgregarAsync(Libro libro);
    Task<ResultadoOperacion> ActualizarAsync(Libro libro);
    Task<bool> EliminarAsync(int id);
}
