using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services.Interfaces;

public interface IAutorService
{
    IReadOnlyList<Autor> ObtenerTodos();
    Autor? ObtenerPorId(int id);
    ResultadoOperacion Agregar(Autor autor);
    ResultadoOperacion Actualizar(Autor autor);
    bool Eliminar(int id);
}
