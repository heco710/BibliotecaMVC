using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories;

public interface IRepositorioAutor
{
    IReadOnlyList<Autor> ObtenerTodos();
    Autor? ObtenerPorId(int id);
    int Agregar(Autor autor);
    bool Actualizar(Autor autor);
    bool Eliminar(int id);
}
