using BibliotecaMVC.Data;
using Microsoft.EntityFrameworkCore;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
}

// Las entradas inválidas deben rechazarse antes de intentar conectar a SQL Server.
using var validationContext = new BibliotecaContext(new DbContextOptionsBuilder<BibliotecaContext>()
    .UseSqlServer("Server=127.0.0.1,1;Database=validation;Integrated Security=True;Connect Timeout=1").Options);
var libroService = new LibroService(new RepositorioLibrosEf(validationContext));
var invalidBook = new Libro { Titulo = "Prueba", Autor = "Autora", Categoria = "Novela", ISBN = "978-123",
    AnioPublicacion = DateTime.Today.Year + 1, Imagen = "ficciones.png" };
Check(!(await libroService.AgregarAsync(invalidBook)).Exitoso, "Rechazar publicación futura antes de SQL");
invalidBook.AnioPublicacion = 2020;
invalidBook.Imagen = "../../secret.txt";
Check(!(await libroService.AgregarAsync(invalidBook)).Exitoso, "Rechazar imagen fuera del catálogo");
invalidBook.Imagen = "ficciones.png";
invalidBook.Titulo = "";
Check(!(await libroService.AgregarAsync(invalidBook)).Exitoso, "Validar DataAnnotations sin MVC");
var autorService = new AutorService(new RepositorioAutoresEf(validationContext));
Check(!(await autorService.AgregarAsync(new Autor { Nombre = "Ana", Apellido = "Pérez", Nacionalidad = "Guatemalteca",
    FechaNacimiento = DateTime.Today.AddDays(1) })).Exitoso, "Rechazar nacimiento futuro antes de SQL");
Check(!validationContext.ChangeTracker.HasChanges(), "La validación no registra cambios en EF");

var fake = new CategoriaFake();
var categoriaService = new CategoriaService(fake);
Check(!(await categoriaService.AgregarAsync(new Categoria { Nombre = " " })).Exitoso, "Nombre obligatorio");
Check(!(await categoriaService.AgregarAsync(new Categoria { Nombre = new string('n', 101) })).Exitoso, "Nombre máximo 100");
Check(!(await categoriaService.ActualizarAsync(new Categoria { Nombre = "Válida", Descripcion = new string('d', 251) })).Exitoso, "Descripción máxima 250");
Check(fake.Writes == 0, "Entradas inválidas no llegan a SQL");
var categoria = new Categoria { Nombre = "  Ciencia ficción  ", Descripcion = "  " };
Check((await categoriaService.AgregarAsync(categoria)).Exitoso && categoria.ID == 7, "Asignar identidad generada");
Check(categoria.Nombre == "Ciencia ficción" && categoria.Descripcion is null, "Normalizar espacios y descripción nula");
Check(!(await categoriaService.ActualizarAsync(categoria)).Encontrado, "Comunicar actualización inexistente");

var connectionString = Environment.GetEnvironmentVariable("BIBLIOTECA_TEST_CONNECTION");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    var builder = new SqlConnectionStringBuilder(connectionString);
    if (!builder.InitialCatalog.StartsWith("BibliotecaMVC_Test_", StringComparison.Ordinal))
        throw new Exception("SQL tests require a dedicated BibliotecaMVC_Test_ database.");
    var options = new DbContextOptionsBuilder<BibliotecaContext>().UseSqlServer(connectionString).Options;
    await using (var schema = new BibliotecaContext(options))
    {
        await schema.Database.MigrateAsync();
        Check(!schema.Database.HasPendingModelChanges(), "Modelo y migración coinciden");
    }
    var book = new Libro { ID = 999, Titulo = "Niñez ' y libros", Autor = "Autora", Categoria = "Novela",
        AnioPublicacion = 2020, ISBN = "978-123", Imagen = "ficciones.png", Disponible = true };
    var author = new Autor { ID = 999, Nombre = "María", Apellido = "O'Neill", Nacionalidad = "Guatemalteca",
        FechaNacimiento = new DateTime(2000, 1, 1), Activo = true };
    int bookCount, authorCount;
    await using (var before = new BibliotecaContext(options))
    {
        bookCount = await before.Libros.CountAsync();
        authorCount = await before.Autores.CountAsync();
    }
    try
    {
        await using (var create = new BibliotecaContext(options))
        {
            Check((await new LibroService(new RepositorioLibrosEf(create)).AgregarAsync(book)).Exitoso && book.ID > 0 && book.ID != 999,
                "EF agrega libro con identidad de SQL, ignorando el ID recibido");
            Check((await new AutorService(new RepositorioAutoresEf(create)).AgregarAsync(author)).Exitoso && author.ID > 0 && author.ID != 999,
                "EF agrega autor con identidad de SQL");
        }
        await using (var read = new BibliotecaContext(options))
        {
            var savedBook = await new RepositorioLibrosEf(read).ObtenerPorIdAsync(book.ID);
            Check(savedBook?.Titulo == book.Titulo && savedBook.Descripcion is null && savedBook.Disponible,
                "EF conserva Unicode, apóstrofes, NULL y disponibilidad entre contextos");
            var savedAuthor = await new RepositorioAutoresEf(read).ObtenerPorIdAsync(author.ID);
            Check(savedAuthor?.Apellido == author.Apellido && savedAuthor.FechaNacimiento == author.FechaNacimiento && savedAuthor.Activo,
                "EF conserva autor y fecha entre contextos");
        }
        book.Titulo = "Libro editado"; book.Descripcion = new string('ñ', 500); book.Disponible = false;
        author.Nombre = "Autora editada"; author.Activo = false;
        await using (var edit = new BibliotecaContext(options))
        {
            Check((await new LibroService(new RepositorioLibrosEf(edit)).ActualizarAsync(book)).Exitoso, "EF UPDATE libro");
            Check((await new AutorService(new RepositorioAutoresEf(edit)).ActualizarAsync(author)).Exitoso, "EF UPDATE autor");
        }
        await using (var read = new BibliotecaContext(options))
        {
            await read.Database.MigrateAsync();
            var savedBook = await read.Libros.FindAsync(book.ID);
            var savedAuthor = await read.Autores.FindAsync(author.ID);
            Check(savedBook?.Titulo == book.Titulo && savedBook.Descripcion == book.Descripcion && !savedBook.Disponible,
                "Edición de libro persiste y repetir migración no la sobrescribe");
            Check(savedAuthor?.Nombre == author.Nombre && !savedAuthor.Activo, "Edición de autor persiste");
            Check((await new RepositorioLibrosEf(read).ObtenerTodosAsync()).Any(item => item.ID == book.ID), "EF listado de libros");
            Check((await new RepositorioAutoresEf(read).ObtenerTodosAsync()).Any(item => item.ID == author.ID), "EF listado de autores");
        }
        await using (var delete = new BibliotecaContext(options))
        {
            Check(await new RepositorioLibrosEf(delete).EliminarAsync(book.ID), "EF DELETE libro");
            Check(await new RepositorioAutoresEf(delete).EliminarAsync(author.ID), "EF DELETE autor");
        }
        await using (var missing = new BibliotecaContext(options))
        {
            var books = new RepositorioLibrosEf(missing); var authors = new RepositorioAutoresEf(missing);
            Check(await books.ObtenerPorIdAsync(book.ID) is null && await authors.ObtenerPorIdAsync(author.ID) is null,
                "Filas eliminadas no existen en un contexto nuevo");
            Check(!await books.ActualizarAsync(book) && !await authors.ActualizarAsync(author), "No insertar al editar un ID inexistente");
            Check(!await books.EliminarAsync(book.ID) && !await authors.EliminarAsync(author.ID), "Eliminar dos veces devuelve no encontrado");
            Check(await missing.Libros.CountAsync() == bookCount && await missing.Autores.CountAsync() == authorCount,
                "Pruebas conservan los registros anteriores");
        }
    }
    finally
    {
        await using var cleanup = new BibliotecaContext(options);
        if (book.ID != 999) await new RepositorioLibrosEf(cleanup).EliminarAsync(book.ID);
        if (author.ID != 999) await new RepositorioAutoresEf(cleanup).EliminarAsync(author.ID);
    }
    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:BibliotecaDB"] = connectionString
    }).Build();
    var repo = new RepositorioCategoriasSql(config);
    var service = new CategoriaService(repo);
    var row = new Categoria { Nombre = "Niñez '); DROP TABLE Categorias;--", Descripcion = null };
    try
    {
        Check((await service.AgregarAsync(row)).Exitoso && row.ID > 0, "SQL INSERT devuelve identidad");
        var read = await repo.ObtenerPorIdAsync(row.ID);
        Check(read?.Nombre == row.Nombre && read.Descripcion is null, "SQL Unicode, apóstrofes y NULL");
        row.Nombre = new string('ñ', 100);
        row.Descripcion = new string('ó', 250);
        Check((await service.ActualizarAsync(row)).Exitoso, "SQL UPDATE acepta límites exactos");
        Check((await new RepositorioCategoriasSql(config).ObtenerPorIdAsync(row.ID))?.Descripcion == row.Descripcion, "Persistencia entre conexiones");
        Check((await repo.ObtenerTodosAsync()).Any(item => item.ID == row.ID), "SQL lista incluye alta");
        Check(await service.EliminarAsync(row.ID), "SQL DELETE");
        Check(await repo.ObtenerPorIdAsync(row.ID) is null, "SQL fila eliminada");
        Check(!(await service.ActualizarAsync(row)).Encontrado && !await service.EliminarAsync(row.ID), "SQL filas inexistentes");
    }
    finally { if (row.ID > 0) await repo.EliminarAsync(row.ID); }
}
else Console.WriteLine("SQL integration skipped: BIBLIOTECA_TEST_CONNECTION not set.");
Console.WriteLine($"PASS: {checks} checks.");

sealed class CategoriaFake : IRepositorioCategoria
{
    public int Writes { get; private set; }
    public Task<IReadOnlyList<Categoria>> ObtenerTodosAsync() => Task.FromResult<IReadOnlyList<Categoria>>([]);
    public Task<Categoria?> ObtenerPorIdAsync(int id) => Task.FromResult<Categoria?>(null);
    public Task<int> AgregarAsync(Categoria categoria) { Writes++; return Task.FromResult(7); }
    public Task<bool> ActualizarAsync(Categoria categoria) { Writes++; return Task.FromResult(false); }
    public Task<bool> EliminarAsync(int id) => Task.FromResult(false);
}
