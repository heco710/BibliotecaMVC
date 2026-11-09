using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BibliotecaMVC.UnitTests;

// LibroService concentra la lógica del módulo Libros: validación, ISBN único, portadas y persistencia.
// El repositorio (EF Core) y el servicio de imágenes (disco) se simulan con Moq, así no se usa SQL Server ni archivos.
public class LibroServiceTests
{
    private readonly Mock<IRepositorioLibro> repositorio = new();
    private readonly Mock<IImagenService> imagenes = new();
    private readonly LibroService servicio;

    public LibroServiceTests() => servicio = new LibroService(repositorio.Object, imagenes.Object);

    private static Libro LibroValido(int id = 0, string imagen = "ficciones.png") => new()
    {
        ID = id, Titulo = "Ficciones", Autor = "Jorge Luis Borges", Categoria = "Cuento",
        AnioPublicacion = 1944, ISBN = "978-84-206-3388-4", Imagen = imagen, Disponible = true
    };

    private static IFormFile Archivo() => new Mock<IFormFile>().Object;

    // ---------- Consulta ----------

    [Fact]
    public async Task ObtenerTodosAsync_ConLibrosRegistrados_DevuelveLosLibrosDelRepositorio()
    {
        repositorio.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync([LibroValido(1), LibroValido(2)]);

        var libros = await servicio.ObtenerTodosAsync();

        Assert.Equal(2, libros.Count);
        Assert.Equal(new[] { 1, 2 }, libros.Select(l => l.ID));
        repositorio.Verify(r => r.ObtenerTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodosAsync_SinLibros_DevuelveColeccionVacia()
    {
        repositorio.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync([]);

        var libros = await servicio.ObtenerTodosAsync();

        Assert.NotNull(libros);
        Assert.Empty(libros);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_LibroExistente_DevuelveElLibro()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(LibroValido(5));

        var libro = await servicio.ObtenerPorIdAsync(5);

        Assert.NotNull(libro);
        Assert.Equal(5, libro.ID);
        Assert.Equal("Ficciones", libro.Titulo);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_LibroInexistente_DevuelveNull()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>())).ReturnsAsync((Libro?)null);

        Assert.Null(await servicio.ObtenerPorIdAsync(404));
    }

    // ---------- Registro ----------

    [Fact]
    public async Task AgregarAsync_LibroValido_GuardaYAsignaElIdGenerado()
    {
        var libro = LibroValido(id: 99); // el ID recibido del formulario se ignora
        repositorio.Setup(r => r.AgregarAsync(libro)).ReturnsAsync(25);

        var resultado = await servicio.AgregarAsync(libro);

        Assert.True(resultado.Exitoso);
        Assert.Empty(resultado.Errores);
        Assert.Equal(25, libro.ID);
        repositorio.Verify(r => r.ExisteIsbnAsync(libro.ISBN, 0), Times.Once);
        repositorio.Verify(r => r.AgregarAsync(libro), Times.Once);
    }

    [Fact]
    public async Task AgregarAsync_IsbnDuplicado_RetornaErrorEnIsbnYNoGuarda()
    {
        repositorio.Setup(r => r.ExisteIsbnAsync(It.IsAny<string>(), 0)).ReturnsAsync(true);

        var resultado = await servicio.AgregarAsync(LibroValido());

        Assert.False(resultado.Exitoso);
        Assert.True(resultado.Encontrado);
        var error = Assert.Single(resultado.Errores);
        Assert.Equal("Ya existe un libro con este ISBN.", error.ErrorMessage);
        Assert.Contains(nameof(Libro.ISBN), error.MemberNames);
        repositorio.Verify(r => r.AgregarAsync(It.IsAny<Libro>()), Times.Never);
    }

    public static TheoryData<string, Action<Libro>, string> DatosInvalidos => new()
    {
        { "Título vacío", l => l.Titulo = "", nameof(Libro.Titulo) },
        { "Título de 151 caracteres", l => l.Titulo = new string('t', 151), nameof(Libro.Titulo) },
        { "Autor vacío", l => l.Autor = "", nameof(Libro.Autor) },
        { "Categoría vacía", l => l.Categoria = "", nameof(Libro.Categoria) },
        { "ISBN vacío", l => l.ISBN = "", nameof(Libro.ISBN) },
        { "ISBN de 21 caracteres", l => l.ISBN = new string('9', 21), nameof(Libro.ISBN) },
        { "Año menor a 1000", l => l.AnioPublicacion = 999, nameof(Libro.AnioPublicacion) },
        { "Año en el futuro", l => l.AnioPublicacion = DateTime.Today.Year + 1, nameof(Libro.AnioPublicacion) },
        { "Descripción de 501 caracteres", l => l.Descripcion = new string('d', 501), nameof(Libro.Descripcion) },
        { "Imagen fuera del catálogo", l => l.Imagen = "../../secret.txt", nameof(Libro.Imagen) },
    };

    [Theory]
    [MemberData(nameof(DatosInvalidos))]
    public async Task AgregarAsync_DatosInvalidos_RetornaErrorEnElCampoYNoConsultaNiGuarda(string caso, Action<Libro> invalidar, string campo)
    {
        var libro = LibroValido();
        invalidar(libro);

        var resultado = await servicio.AgregarAsync(libro);

        Assert.False(resultado.Exitoso, caso);
        Assert.True(resultado.Encontrado);
        Assert.Contains(resultado.Errores, e => e.MemberNames.Contains(campo));
        repositorio.Verify(r => r.ExisteIsbnAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        repositorio.Verify(r => r.AgregarAsync(It.IsAny<Libro>()), Times.Never);
    }

    public static TheoryData<int> AniosLimiteValidos => new() { 1000, DateTime.Today.Year };

    [Theory]
    [MemberData(nameof(AniosLimiteValidos))]
    public async Task AgregarAsync_AnioEnElLimitePermitido_EsAceptado(int anio)
    {
        var libro = LibroValido();
        libro.AnioPublicacion = anio;

        Assert.True((await servicio.AgregarAsync(libro)).Exitoso);
    }

    [Theory]
    [InlineData("cien-anos-soledad.png")]
    [InlineData("casa-espiritus.png")]
    [InlineData("ficciones.png")]
    [InlineData("senor-presidente.png")]
    public async Task AgregarAsync_PortadaDelCatalogo_EsAceptada(string portada)
    {
        Assert.True((await servicio.AgregarAsync(LibroValido(imagen: portada))).Exitoso);
    }

    [Fact]
    public async Task AgregarAsync_ArchivoDeImagenValido_GuardaLaImagenYLaAsignaAlLibro()
    {
        var archivo = Archivo();
        imagenes.Setup(i => i.Validar(archivo)).Returns((string?)null);
        imagenes.Setup(i => i.GuardarAsync(archivo)).ReturnsAsync("libros/nueva.png");
        repositorio.Setup(r => r.AgregarAsync(It.IsAny<Libro>())).ReturnsAsync(1);
        var libro = LibroValido(); // el formulario siempre envía una portada del select; el archivo la reemplaza

        var resultado = await servicio.AgregarAsync(libro, archivo);

        Assert.True(resultado.Exitoso);
        Assert.Equal("libros/nueva.png", libro.Imagen);
        imagenes.Verify(i => i.GuardarAsync(archivo), Times.Once);
        imagenes.Verify(i => i.Eliminar(It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task AgregarAsync_ArchivoDeImagenInvalido_RetornaErrorYNoGuardaNada()
    {
        var archivo = Archivo();
        imagenes.Setup(i => i.Validar(archivo)).Returns("La imagen debe ser JPG, PNG o WEBP.");

        var resultado = await servicio.AgregarAsync(LibroValido(), archivo);

        Assert.False(resultado.Exitoso);
        Assert.Contains(resultado.Errores, e => e.MemberNames.Contains("imagenArchivo"));
        imagenes.Verify(i => i.GuardarAsync(It.IsAny<IFormFile>()), Times.Never);
        repositorio.Verify(r => r.AgregarAsync(It.IsAny<Libro>()), Times.Never);
    }

    [Fact]
    public async Task AgregarAsync_FallaLaBaseDeDatos_EliminaLaImagenSubidaParaNoDejarHuerfanos()
    {
        var archivo = Archivo();
        imagenes.Setup(i => i.GuardarAsync(archivo)).ReturnsAsync("libros/huerfana.png");
        repositorio.Setup(r => r.AgregarAsync(It.IsAny<Libro>())).ThrowsAsync(new InvalidOperationException("SQL no disponible"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.AgregarAsync(LibroValido(), archivo));

        imagenes.Verify(i => i.Eliminar("libros/huerfana.png"), Times.Once);
    }

    // ---------- Edición ----------

    [Fact]
    public async Task ActualizarAsync_LibroInexistente_RetornaNoEncontradoSinGuardar()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync((Libro?)null);

        var resultado = await servicio.ActualizarAsync(LibroValido(7));

        Assert.False(resultado.Encontrado);
        Assert.False(resultado.Exitoso);
        repositorio.Verify(r => r.ActualizarAsync(It.IsAny<Libro>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_DatosValidos_ActualizaSinBorrarLaImagenActual()
    {
        var libro = LibroValido(3);
        libro.Titulo = "Ficciones (edición revisada)";
        repositorio.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(LibroValido(3));
        repositorio.Setup(r => r.ActualizarAsync(libro)).ReturnsAsync(true);

        var resultado = await servicio.ActualizarAsync(libro);

        Assert.True(resultado.Exitoso);
        repositorio.Verify(r => r.ExisteIsbnAsync(libro.ISBN, 3), Times.Once); // excluye al propio libro
        repositorio.Verify(r => r.ActualizarAsync(libro), Times.Once);
        imagenes.Verify(i => i.Eliminar(It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_ConservaUnaImagenSubidaPropia_EsAceptadaAunqueNoSeaDelCatalogo()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(LibroValido(3, "libros/propia.png"));
        repositorio.Setup(r => r.ActualizarAsync(It.IsAny<Libro>())).ReturnsAsync(true);

        var resultado = await servicio.ActualizarAsync(LibroValido(3, "libros/propia.png"));

        Assert.True(resultado.Exitoso);
        imagenes.Verify(i => i.Eliminar(It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_CambiaAUnaPortadaDelCatalogo_EliminaLaImagenAnterior()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(LibroValido(3, "libros/vieja.png"));
        repositorio.Setup(r => r.ActualizarAsync(It.IsAny<Libro>())).ReturnsAsync(true);

        var resultado = await servicio.ActualizarAsync(LibroValido(3, "senor-presidente.png"));

        Assert.True(resultado.Exitoso);
        imagenes.Verify(i => i.Eliminar("libros/vieja.png"), Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_ImagenDeOtroLibroSubido_EsRechazada()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(LibroValido(3, "ficciones.png"));

        var resultado = await servicio.ActualizarAsync(LibroValido(3, "libros/de-otro-libro.png"));

        Assert.False(resultado.Exitoso);
        Assert.Contains(resultado.Errores, e => e.MemberNames.Contains(nameof(Libro.Imagen)));
        repositorio.Verify(r => r.ActualizarAsync(It.IsAny<Libro>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_IsbnDeOtroLibro_RetornaErrorYNoGuarda()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(LibroValido(3));
        repositorio.Setup(r => r.ExisteIsbnAsync(It.IsAny<string>(), 3)).ReturnsAsync(true);

        var resultado = await servicio.ActualizarAsync(LibroValido(3));

        Assert.False(resultado.Exitoso);
        Assert.Contains(resultado.Errores, e => e.MemberNames.Contains(nameof(Libro.ISBN)));
        repositorio.Verify(r => r.ActualizarAsync(It.IsAny<Libro>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(DatosInvalidos))]
    public async Task ActualizarAsync_DatosInvalidos_RetornaErrorYNoGuarda(string caso, Action<Libro> invalidar, string campo)
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(LibroValido(3));
        var libro = LibroValido(3);
        invalidar(libro);

        var resultado = await servicio.ActualizarAsync(libro);

        Assert.False(resultado.Exitoso, caso);
        Assert.Contains(resultado.Errores, e => e.MemberNames.Contains(campo));
        repositorio.Verify(r => r.ActualizarAsync(It.IsAny<Libro>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarAsync_ElLibroSeEliminaAntesDeGuardar_BorraLaImagenNuevaYRetornaNoEncontrado()
    {
        var archivo = Archivo();
        imagenes.Setup(i => i.GuardarAsync(archivo)).ReturnsAsync("libros/nueva.png");
        repositorio.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(LibroValido(3, "libros/vieja.png"));
        repositorio.Setup(r => r.ActualizarAsync(It.IsAny<Libro>())).ReturnsAsync(false);

        var resultado = await servicio.ActualizarAsync(LibroValido(3, "libros/vieja.png"), archivo);

        Assert.False(resultado.Encontrado);
        imagenes.Verify(i => i.Eliminar("libros/nueva.png"), Times.Once);
        imagenes.Verify(i => i.Eliminar("libros/vieja.png"), Times.Never);
    }

    // ---------- Eliminación ----------

    [Fact]
    public async Task EliminarAsync_LibroExistente_EliminaElRegistroYSuImagen()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(8)).ReturnsAsync(LibroValido(8, "libros/portada.png"));
        repositorio.Setup(r => r.EliminarAsync(8)).ReturnsAsync(true);

        Assert.True(await servicio.EliminarAsync(8));

        repositorio.Verify(r => r.EliminarAsync(8), Times.Once);
        imagenes.Verify(i => i.Eliminar("libros/portada.png"), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_LibroInexistente_RetornaFalseSinEliminarNada()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>())).ReturnsAsync((Libro?)null);

        Assert.False(await servicio.EliminarAsync(404));

        repositorio.Verify(r => r.EliminarAsync(It.IsAny<int>()), Times.Never);
        imagenes.Verify(i => i.Eliminar(It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task EliminarAsync_LaBaseNoEliminaElRegistro_ConservaLaImagen()
    {
        repositorio.Setup(r => r.ObtenerPorIdAsync(8)).ReturnsAsync(LibroValido(8, "libros/portada.png"));
        repositorio.Setup(r => r.EliminarAsync(8)).ReturnsAsync(false);

        Assert.False(await servicio.EliminarAsync(8));

        imagenes.Verify(i => i.Eliminar(It.IsAny<string?>()), Times.Never);
    }
}
