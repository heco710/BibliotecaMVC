using BibliotecaMVC.Services;

namespace BibliotecaMVC.UnitTests;

// Primera prueba: confirma que xUnit se ejecuta y que el proyecto de pruebas referencia a BibliotecaMVC.
public class EntornoTests
{
    [Fact]
    public void ProyectoDePruebas_ReferenciaBibliotecaMVC_PuedeLeerElCatalogoDePortadas()
    {
        Assert.Equal(4, LibroService.Portadas.Length);
        Assert.Contains("ficciones.png", LibroService.Portadas);
    }
}
