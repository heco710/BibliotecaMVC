using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class AutoresController : Controller
    {
        public IActionResult Index()
        {
            List<Autor> autores = new List<Autor>
            {
                new Autor
                {
                    ID = 1,
                    Nombre = "Gabriel",
                    Apellido = "García Márquez",
                    Nacionalidad = "Colombiana",
                    FechaNacimiento = new DateTime(1927, 3, 6),
                    Activo = false
                },
                new Autor
                {
                    ID = 2,
                    Nombre = "Isabel",
                    Apellido = "Allende",
                    Nacionalidad = "Chilena",
                    FechaNacimiento = new DateTime(1942, 8, 2),
                    Activo = true
                },
                new Autor
                {
                    ID = 3,
                    Nombre = "Jorge Luis",
                    Apellido = "Borges",
                    Nacionalidad = "Argentina",
                    FechaNacimiento = new DateTime(1899, 8, 24),
                    Activo = false
                },
                new Autor
                {
                    ID = 4,
                    Nombre = "Laura",
                    Apellido = "Esquivel",
                    Nacionalidad = "Mexicana",
                    FechaNacimiento = new DateTime(1950, 9, 30),
                    Activo = true
                },
                new Autor
                {
                    ID = 5,
                    Nombre = "Miguel Ángel",
                    Apellido = "Asturias",
                    Nacionalidad = "Guatemalteca",
                    FechaNacimiento = new DateTime(1899, 10, 19),
                    Activo = true
                }
            };

            return View(autores);
        }
    }
}
