using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services.Interfaces;

public interface ILibroService
{
    Task<IReadOnlyList<Libro>> ObtenerTodosAsync();
    Task<Libro?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion> AgregarAsync(Libro libro, IFormFile? archivo = null);
    Task<ResultadoOperacion> ActualizarAsync(Libro libro, IFormFile? archivo = null);
    Task<bool> EliminarAsync(int id);
}
