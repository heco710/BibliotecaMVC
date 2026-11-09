using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Controllers;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BibliotecaMVC.UnitTests;

// El controlador se prueba con ILibroService simulado: se verifica qué resultado HTTP devuelve
// y cuándo invoca al servicio, sin base de datos, vistas ni autenticación reales.
public class LibrosControllerTests
{
    private readonly Mock<ILibroService> servicio = new();
    private readonly LibrosController controller;

    public LibrosControllerTests()
    {
        var http = new DefaultHttpContext();
        controller = new LibrosController(servicio.Object, NullLogger<LibrosController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = http },
            TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>())
        };
    }

    private static Libro Libro(int id = 1) => new()
    {
        ID = id, Titulo = "El Señor Presidente", Autor = "Miguel Ángel Asturias", Categoria = "Novela",
        AnioPublicacion = 1946, ISBN = "978-84-376-0001-1", Imagen = "senor-presidente.png"
    };

    private static ResultadoOperacion Exito => new(true, []);

    [Fact]
    public async Task Index_ConLibros_MuestraLaVistaConElListado()
    {
        servicio.Setup(s => s.ObtenerTodosAsync()).ReturnsAsync([Libro(1), Libro(2)]);

        var vista = Assert.IsType<ViewResult>(await controller.Index());

        var modelo = Assert.IsAssignableFrom<IReadOnlyList<Libro>>(vista.Model);
        Assert.Equal(2, modelo.Count);
    }

    [Fact]
    public async Task Index_SinLibros_MuestraLaVistaConListadoVacio()
    {
        servicio.Setup(s => s.ObtenerTodosAsync()).ReturnsAsync([]);

        var vista = Assert.IsType<ViewResult>(await controller.Index());

        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<Libro>>(vista.Model));
    }

    [Fact]
    public async Task Details_LibroExistente_MuestraElLibro()
    {
        servicio.Setup(s => s.ObtenerPorIdAsync(4)).ReturnsAsync(Libro(4));

        var vista = Assert.IsType<ViewResult>(await controller.Details(4));

        Assert.Equal(4, Assert.IsType<Libro>(vista.Model).ID);
    }

    [Fact]
    public async Task Details_LibroInexistente_RetornaNotFound()
    {
        servicio.Setup(s => s.ObtenerPorIdAsync(It.IsAny<int>())).ReturnsAsync((Libro?)null);

        Assert.IsType<NotFoundResult>(await controller.Details(404));
    }

    [Fact]
    public void Create_Get_PreparaUnLibroDisponibleDelAnioActual()
    {
        var vista = Assert.IsType<ViewResult>(controller.Create());

        var libro = Assert.IsType<Libro>(vista.Model);
        Assert.True(libro.Disponible);
        Assert.Equal(DateTime.Today.Year, libro.AnioPublicacion);
    }

    [Fact]
    public async Task Create_Post_LibroValido_RedirigeAlListadoConMensaje()
    {
        var libro = Libro(0);
        servicio.Setup(s => s.AgregarAsync(libro, null)).ReturnsAsync(Exito);

        var redireccion = Assert.IsType<RedirectToActionResult>(await controller.Create(libro, null));

        Assert.Equal(nameof(LibrosController.Index), redireccion.ActionName);
        Assert.Equal("El libro se agregó correctamente.", controller.TempData["Mensaje"]);
        servicio.Verify(s => s.AgregarAsync(libro, null), Times.Once);
    }

    [Fact]
    public async Task Create_Post_ModelStateInvalido_VuelveAlFormularioSinLlamarAlServicio()
    {
        controller.ModelState.AddModelError(nameof(Models.Libro.Titulo), "El título es obligatorio.");

        var vista = Assert.IsType<ViewResult>(await controller.Create(Libro(0), null));

        Assert.False(controller.ModelState.IsValid);
        Assert.NotNull(vista.Model);
        servicio.Verify(s => s.AgregarAsync(It.IsAny<Libro>(), It.IsAny<IFormFile?>()), Times.Never);
    }

    [Fact]
    public async Task Create_Post_ServicioRechazaIsbn_MuestraElErrorEnElCampo()
    {
        servicio.Setup(s => s.AgregarAsync(It.IsAny<Libro>(), null))
            .ReturnsAsync(new ResultadoOperacion(true, [new ValidationResult("Ya existe un libro con este ISBN.", [nameof(Models.Libro.ISBN)])]));

        Assert.IsType<ViewResult>(await controller.Create(Libro(0), null));

        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Models.Libro.ISBN)));
        Assert.Null(controller.TempData["Mensaje"]);
    }

    [Fact]
    public async Task Edit_Get_LibroInexistente_RetornaNotFound()
    {
        servicio.Setup(s => s.ObtenerPorIdAsync(It.IsAny<int>())).ReturnsAsync((Libro?)null);

        Assert.IsType<NotFoundResult>(await controller.Edit(404));
    }

    [Fact]
    public async Task Edit_Post_IdDeRutaDistintoAlDelFormulario_RetornaBadRequest()
    {
        Assert.IsType<BadRequestResult>(await controller.Edit(1, Libro(2), null));

        servicio.Verify(s => s.ActualizarAsync(It.IsAny<Libro>(), It.IsAny<IFormFile?>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Post_LibroEliminadoMientrasSeEditaba_RetornaNotFound()
    {
        servicio.Setup(s => s.ActualizarAsync(It.IsAny<Libro>(), null)).ReturnsAsync(new ResultadoOperacion(false, []));

        Assert.IsType<NotFoundResult>(await controller.Edit(3, Libro(3), null));
    }

    [Fact]
    public async Task Edit_Post_LibroValido_RedirigeAlListadoConMensaje()
    {
        var libro = Libro(3);
        servicio.Setup(s => s.ActualizarAsync(libro, null)).ReturnsAsync(Exito);

        var redireccion = Assert.IsType<RedirectToActionResult>(await controller.Edit(3, libro, null));

        Assert.Equal(nameof(LibrosController.Index), redireccion.ActionName);
        Assert.Equal("La información del libro se actualizó correctamente.", controller.TempData["Mensaje"]);
        servicio.Verify(s => s.ActualizarAsync(libro, null), Times.Once);
    }

    [Fact]
    public async Task Delete_Get_LibroInexistente_RetornaNotFound()
    {
        servicio.Setup(s => s.ObtenerPorIdAsync(It.IsAny<int>())).ReturnsAsync((Libro?)null);

        Assert.IsType<NotFoundResult>(await controller.Delete(404));
    }

    [Fact]
    public async Task DeleteConfirmed_LibroExistente_RedirigeAlListadoConMensaje()
    {
        servicio.Setup(s => s.EliminarAsync(6)).ReturnsAsync(true);

        var redireccion = Assert.IsType<RedirectToActionResult>(await controller.DeleteConfirmed(6));

        Assert.Equal(nameof(LibrosController.Index), redireccion.ActionName);
        Assert.Equal("El libro se eliminó correctamente.", controller.TempData["Mensaje"]);
        servicio.Verify(s => s.EliminarAsync(6), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_LibroInexistente_RetornaNotFound()
    {
        servicio.Setup(s => s.EliminarAsync(It.IsAny<int>())).ReturnsAsync(false);

        Assert.IsType<NotFoundResult>(await controller.DeleteConfirmed(404));
    }
}
