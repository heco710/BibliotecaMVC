using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories;

public interface IRepositorioLibro
{
    IReadOnlyList<Libro> ObtenerTodos();
    Libro? ObtenerPorId(int id);
    int Agregar(Libro libro);
    bool Actualizar(Libro libro);
    bool Eliminar(int id);
}
