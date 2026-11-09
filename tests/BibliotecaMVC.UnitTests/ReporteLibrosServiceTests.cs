using BibliotecaMVC.Controllers;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services;
using BibliotecaMVC.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BibliotecaMVC.UnitTests;

// Reportería: los repositorios se simulan con Moq; el PDF y el Excel se generan de verdad en memoria
// y el Excel se vuelve a abrir con ClosedXML para comprobar que refleja los filtros.
public class ReporteLibrosServiceTests
{
    private readonly Mock<IRepositorioLibro> libros = new();
    private readonly Mock<IRepositorioAutor> autores = new();
    private readonly ReporteLibrosService servicio;

    public ReporteLibrosServiceTests()
    {
        libros.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(
        [
            new Libro { ID = 1, Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Novela", AnioPublicacion = 1967, ISBN = "978-1", Disponible = true },
            new Libro { ID = 2, Titulo = "Ficciones", Autor = "Jorge Luis Borges", Categoria = "Cuento", AnioPublicacion = 1944, ISBN = "978-2", Disponible = false },
            new Libro { ID = 3, Titulo = "El Aleph", Autor = "Jorge Luis Borges", Categoria = "Cuento", AnioPublicacion = 1949, ISBN = "978-3", Disponible = true },
        ]);
        autores.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(
        [
            new Autor { ID = 1, Nombre = "Jorge Luis", Apellido = "Borges", Nacionalidad = "Argentina" },
        ]);
        servicio = new ReporteLibrosService(libros.Object, autores.Object);
    }

    [Fact]
    public async Task ObtenerAsync_SinFiltros_IncluyeTodosLosLibrosYElResumen()
    {
        var reporte = await servicio.ObtenerAsync(null, " ");

        Assert.Equal(3, reporte.Total);
        Assert.Equal(2, reporte.Disponibles);
        Assert.Equal(1, reporte.Prestados);
        Assert.Equal(2, reporte.TotalAutores);
        Assert.Null(reporte.Categoria);
        Assert.Equal(new[] { "Cuento", "Novela" }, reporte.CategoriasDisponibles);
        libros.Verify(r => r.ObtenerTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task ObtenerAsync_FiltroPorAutor_SoloIncluyeSusLibrosConNacionalidad()
    {
        var reporte = await servicio.ObtenerAsync("jorge luis borges", null);

        Assert.Equal(2, reporte.Total);
        Assert.All(reporte.Filas, fila => Assert.Equal("Argentina", fila.Nacionalidad));
        Assert.Equal(2, reporte.AutoresDisponibles.Count); // las opciones del filtro no se reducen al filtrar
    }

    [Fact]
    public async Task ObtenerAsync_AutorSinRegistroEnAutores_DejaNacionalidadVacia()
    {
        var reporte = await servicio.ObtenerAsync(null, "Novela");

        var fila = Assert.Single(reporte.Filas);
        Assert.Null(fila.Nacionalidad);
    }

    [Fact]
    public async Task ObtenerAsync_FiltrosSinCoincidencias_DevuelveReporteVacio()
    {
        var reporte = await servicio.ObtenerAsync("Jorge Luis Borges", "Novela");

        Assert.Empty(reporte.Filas);
        Assert.Equal(0, reporte.Total);
    }

    [Fact]
    public async Task GenerarExcel_ConFiltro_CreaLaHojaConEncabezadosYSoloLosLibrosFiltrados()
    {
        var reporte = await servicio.ObtenerAsync(null, "Cuento");

        using var excel = new XLWorkbook(new MemoryStream(servicio.GenerarExcel(reporte)));

        var hoja = excel.Worksheet("Reporte de Libros");
        Assert.Equal(new[] { "ID", "Título", "Autor", "Nacionalidad", "Categoría", "Año", "ISBN", "Estado" },
            hoja.Range(6, 1, 6, 8).Cells().Select(celda => celda.GetString()));
        Assert.True(hoja.Cell(6, 1).Style.Font.Bold);
        Assert.Equal("Autor: Todos · Categoría: Cuento", hoja.Cell(3, 3).GetString());
        Assert.Equal(reporte.FechaGeneracion, hoja.Cell(2, 3).GetDateTime(), TimeSpan.FromSeconds(1));
        Assert.Equal(new[] { "Ficciones", "El Aleph" }, new[] { hoja.Cell(7, 2).GetString(), hoja.Cell(8, 2).GetString() });
        Assert.True(hoja.Cell(9, 1).IsEmpty());
    }

    [Fact]
    public async Task GenerarExcel_SinCoincidencias_IndicaQueNoHayLibros()
    {
        var reporte = await servicio.ObtenerAsync("Jorge Luis Borges", "Novela");

        using var excel = new XLWorkbook(new MemoryStream(servicio.GenerarExcel(reporte)));

        Assert.Equal("No hay libros que coincidan con los filtros aplicados.", excel.Worksheet(1).Cell(7, 1).GetString());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Jorge Luis Borges", "Novela")]
    public async Task GenerarPdf_ConYSinResultados_DevuelveUnDocumentoPdfValido(string? autor, string? categoria)
    {
        var pdf = servicio.GenerarPdf(await servicio.ObtenerAsync(autor, categoria));

        Assert.True(pdf.Length > 1000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task ReportesController_DescargaPdfYExcelConNombreYTipoCorrectos()
    {
        var controller = new ReportesController(servicio, NullLogger<ReportesController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var pdf = Assert.IsType<FileContentResult>(await controller.LibrosPdf("Jorge Luis Borges", null));
        var excel = Assert.IsType<FileContentResult>(await controller.LibrosExcel(null, "Cuento"));

        Assert.Equal(("application/pdf", "ReporteLibros.pdf"), (pdf.ContentType, pdf.FileDownloadName));
        Assert.Equal((ReportesController.TipoExcel, "ReporteLibros.xlsx"), (excel.ContentType, excel.FileDownloadName));
    }
}
