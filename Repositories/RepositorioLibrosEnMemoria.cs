using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories;

public sealed class RepositorioLibrosEnMemoria : IRepositorioLibro
{
    // honey: catálogo académico en memoria; sustituir por persistencia en la etapa EF Core.
    private readonly object syncRoot = new();
    private int siguienteId = 5;
    private readonly List<Libro> registros = [
        new() { ID = 1, Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Realismo mágico", AnioPublicacion = 1967, ISBN = "978-0307474728", Descripcion = "La historia de la familia Buendía y del inolvidable pueblo de Macondo.", Imagen = "cien-anos-soledad.png", Disponible = true },
        new() { ID = 2, Titulo = "La casa de los espíritus", Autor = "Isabel Allende", Categoria = "Narrativa", AnioPublicacion = 1982, ISBN = "978-0525433477", Descripcion = "Una saga familiar atravesada por la memoria, el amor y los cambios históricos.", Imagen = "casa-espiritus.png", Disponible = true },
        new() { ID = 3, Titulo = "Ficciones", Autor = "Jorge Luis Borges", Categoria = "Cuentos", AnioPublicacion = 1944, ISBN = "978-0802130303", Descripcion = "Laberintos, bibliotecas y mundos posibles en una colección esencial.", Imagen = "ficciones.png", Disponible = false },
        new() { ID = 4, Titulo = "El señor Presidente", Autor = "Miguel Ángel Asturias", Categoria = "Novela", AnioPublicacion = 1946, ISBN = "978-8420674209", Descripcion = "Una obra fundamental de la literatura guatemalteca sobre el poder y sus sombras.", Imagen = "senor-presidente.png", Disponible = true }
    ];

    public IReadOnlyList<Libro> ObtenerTodos()
    {
        lock (syncRoot) return registros.Select(Copiar).ToList();
    }

    public Libro? ObtenerPorId(int id)
    {
        lock (syncRoot)
        {
            var valor = registros.FirstOrDefault(item => item.ID == id);
            return valor is null ? null : Copiar(valor);
        }
    }

    public int Agregar(Libro libro)
    {
        lock (syncRoot)
        {
            var copia = Copiar(libro);
            copia.ID = siguienteId++;
            registros.Add(copia);
            return copia.ID;
        }
    }

    public bool Actualizar(Libro libro)
    {
        lock (syncRoot)
        {
            var indice = registros.FindIndex(item => item.ID == libro.ID);
            if (indice < 0) return false;
            registros[indice] = Copiar(libro);
            return true;
        }
    }

    public bool Eliminar(int id)
    {
        lock (syncRoot) return registros.RemoveAll(item => item.ID == id) == 1;
    }

    private static Libro Copiar(Libro valor) => new()
    {
        ID = valor.ID,
        Titulo = valor.Titulo,
        Autor = valor.Autor,
        Categoria = valor.Categoria,
        AnioPublicacion = valor.AnioPublicacion,
        ISBN = valor.ISBN,
        Descripcion = valor.Descripcion,
        Imagen = valor.Imagen,
        Disponible = valor.Disponible
    };
}
