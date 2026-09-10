using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories;

public sealed class RepositorioAutoresEnMemoria : IRepositorioAutor
{
    // honey: catálogo académico en memoria; sustituir por persistencia en la etapa EF Core.
    private readonly object syncRoot = new();
    private int siguienteId = 6;
    private readonly List<Autor> registros = [
        new() { ID = 1, Nombre = "Gabriel", Apellido = "García Márquez", Nacionalidad = "Colombiana", FechaNacimiento = new DateTime(1927, 3, 6), Activo = false },
        new() { ID = 2, Nombre = "Isabel", Apellido = "Allende", Nacionalidad = "Chilena", FechaNacimiento = new DateTime(1942, 8, 2), Activo = true },
        new() { ID = 3, Nombre = "Jorge Luis", Apellido = "Borges", Nacionalidad = "Argentina", FechaNacimiento = new DateTime(1899, 8, 24), Activo = false },
        new() { ID = 4, Nombre = "Laura", Apellido = "Esquivel", Nacionalidad = "Mexicana", FechaNacimiento = new DateTime(1950, 9, 30), Activo = true },
        new() { ID = 5, Nombre = "Miguel Ángel", Apellido = "Asturias", Nacionalidad = "Guatemalteca", FechaNacimiento = new DateTime(1899, 10, 19), Activo = false }
    ];

    public IReadOnlyList<Autor> ObtenerTodos()
    {
        lock (syncRoot) return registros.Select(Copiar).ToList();
    }

    public Autor? ObtenerPorId(int id)
    {
        lock (syncRoot)
        {
            var valor = registros.FirstOrDefault(item => item.ID == id);
            return valor is null ? null : Copiar(valor);
        }
    }

    public int Agregar(Autor autor)
    {
        lock (syncRoot)
        {
            var copia = Copiar(autor);
            copia.ID = siguienteId++;
            registros.Add(copia);
            return copia.ID;
        }
    }

    public bool Actualizar(Autor autor)
    {
        lock (syncRoot)
        {
            var indice = registros.FindIndex(item => item.ID == autor.ID);
            if (indice < 0) return false;
            registros[indice] = Copiar(autor);
            return true;
        }
    }

    public bool Eliminar(int id)
    {
        lock (syncRoot) return registros.RemoveAll(item => item.ID == id) == 1;
    }

    private static Autor Copiar(Autor valor) => new()
    {
        ID = valor.ID,
        Nombre = valor.Nombre,
        Apellido = valor.Apellido,
        Nacionalidad = valor.Nacionalidad,
        FechaNacimiento = valor.FechaNacimiento,
        Activo = valor.Activo
    };
}
