using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BibliotecaMVC.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Libros()
        {
            return View();
        }

        public IActionResult Autores()
        {
            return View();
        }

        public IActionResult Categorias()
        {
            return RedirectToAction("Index", "Categorias");
        }
        public IActionResult Acerca_de()
        {
            return View();
        }
        public IActionResult Creditos() => View();

        public IActionResult Prestamos()
        {
            return View();
        }
        public IActionResult Usuarios()
        {
            return View();
        }

        // Destino de UseExceptionHandler. El middleware ya registra la excepción completa; aquí se registra la
        // referencia que ve el usuario para poder relacionar su reporte con esa entrada del registro.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var fallo = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (fallo is not null)
            {
                logger.LogError("Excepción no controlada ({Tipo}) en {Ruta}; se mostró la página de error con la referencia {Referencia}.",
                    fallo.Error.GetType().Name, fallo.Path, requestId);
            }
            return View(new ErrorViewModel { RequestId = requestId });
        }

        // Destino de UseStatusCodePagesWithReExecute para respuestas sin contenido (404, 400...).
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Estado(int id)
        {
            var original = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (original is not null && id == StatusCodes.Status404NotFound)
                logger.LogWarning("Recurso no encontrado: {Ruta}.", original.OriginalPath);
            if (id is >= 400 and <= 599) Response.StatusCode = id;
            return View(id);
        }
    }
}
