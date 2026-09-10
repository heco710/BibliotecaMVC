using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services.Interfaces;

public interface ILibroService
{
    IReadOnlyList<Libro> ObtenerTodos();
    Libro? ObtenerPorId(int id);
    ResultadoOperacion Agregar(Libro libro);
    ResultadoOperacion Actualizar(Libro libro);
    bool Eliminar(int id);
}
