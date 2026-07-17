using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BibliotecaMVC.Controllers
{
    public class HomeController : Controller
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
            return View();
        }
        public IActionResult Acerca_de()
        {
            return View();
        }
        public IActionResult Prestamos()
        {
            return View();
        }
        public IActionResult Usuarios()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
