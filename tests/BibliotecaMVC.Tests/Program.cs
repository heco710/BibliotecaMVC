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

var libros = new RepositorioLibrosEnMemoria();
var libroService = new LibroService(libros);
var libro = libros.ObtenerPorId(1)!;
libro.Titulo = "Copia modificada";
Check(libros.ObtenerPorId(1)!.Titulo != libro.Titulo, "Lectura de libro debe ser una copia");
libros.ObtenerTodos()[0].Titulo = "No modificar almacenamiento";
Check(libros.ObtenerPorId(1)!.Titulo != "No modificar almacenamiento", "Lista debe contener copias");
var firstId = libros.Agregar(libro);
libro.Titulo = "Modificado después de guardar";
Check(libros.ObtenerPorId(firstId)!.Titulo != libro.Titulo, "Agregar debe copiar la entrada");
Check(libros.Eliminar(firstId), "Eliminar existente");
Check(libros.Agregar(libro) > firstId, "No reutilizar ID eliminado");
var ids = new System.Collections.Concurrent.ConcurrentBag<int>();
Parallel.For(0, 100, _ => ids.Add(libros.Agregar(libro)));
Check(ids.Distinct().Count() == 100, "Altas concurrentes deben tener IDs únicos");
Check(!libros.Actualizar(new Libro { ID = int.MaxValue }), "Actualizar inexistente");
Check(!libros.Eliminar(int.MaxValue), "Eliminar inexistente");
var invalidBook = libros.ObtenerPorId(1)!;
invalidBook.AnioPublicacion = DateTime.Today.Year + 1;
Check(!libroService.Actualizar(invalidBook).Exitoso, "Rechazar publicación futura");
Check(libros.ObtenerPorId(1)!.AnioPublicacion != invalidBook.AnioPublicacion, "Validación no debe mutar datos");
invalidBook.AnioPublicacion = 2020;
invalidBook.Imagen = "../../secret.txt";
Check(!libroService.Agregar(invalidBook).Exitoso, "Rechazar imagen fuera del catálogo");
invalidBook.Imagen = "ficciones.png";
invalidBook.Titulo = "";
Check(!libroService.Agregar(invalidBook).Exitoso, "Validar DataAnnotations sin MVC");
invalidBook.Titulo = "Descripción opcional";
invalidBook.Descripcion = null;
Check(libroService.Agregar(invalidBook).Exitoso, "Descripción de libro opcional");

var autores = new RepositorioAutoresEnMemoria();
var autorService = new AutorService(autores);
var autor = autores.ObtenerPorId(1)!;
autor.Nombre = "Copia";
Check(autores.ObtenerPorId(1)!.Nombre != autor.Nombre, "Autor debe ser una copia");
autores.ObtenerTodos()[0].Apellido = "Copia de lista";
Check(autores.ObtenerPorId(1)!.Apellido != "Copia de lista", "Lista de autores debe copiar");
autor.FechaNacimiento = DateTime.Today.AddDays(1);
Check(!autorService.Actualizar(autor).Exitoso, "Rechazar nacimiento futuro");
Check(autores.ObtenerPorId(1)!.Nombre != "Copia", "Autor inválido no debe mutar datos");
autor.FechaNacimiento = new DateTime(2000, 1, 1);
var autorId = autores.Agregar(autor);
autor.Nombre = "Cambio después de guardar";
Check(autores.ObtenerPorId(autorId)!.Nombre != autor.Nombre, "Agregar autor debe copiar");
Check(autores.Eliminar(autorId) && autores.Agregar(autor) > autorId, "IDs de autor no se reutilizan");
ids.Clear();
Parallel.For(0, 100, _ => ids.Add(autores.Agregar(autor)));
Check(ids.Distinct().Count() == 100, "IDs concurrentes de autor");

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
