using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.ViewComponents;

// View Component: combina la lógica (qué secciones permite el rol actual) con su vista
// (Views/Shared/Components/SeccionesDisponibles/Default.cshtml). Se usa en las páginas de error y acceso
// denegado para que el usuario continúe solo hacia secciones a las que sí puede entrar.
public sealed class SeccionesDisponiblesViewComponent : ViewComponent
{
    public sealed record Seccion(string Titulo, string Descripcion, string Controlador, string Accion);

    public IViewComponentResult Invoke()
    {
        var secciones = new List<Seccion>
        {
            new("Categorías", "La colección organizada por temas.", "Categorias", "Index"),
            new("Autores", "Fotografías y datos de los escritores.", "Autores", "Index")
        };
        // Mismas reglas que [Authorize] en los controladores: Libros exige un rol y Roles solo el Admin.
        if (Roles.Lista.Any(User.IsInRole))
            secciones.Insert(0, new("Libros", "Obras, descripciones y disponibilidad.", "Libros", "Index"));
        if (User.IsInRole(Roles.Admin))
            secciones.Add(new("Roles", "Asignación de permisos a los usuarios.", "Usuarios", "Index"));
        if (User.Identity?.IsAuthenticated != true)
            secciones.Add(new("Iniciar sesión", "Accede con tu cuenta para consultar el catálogo.", "Account", "Login"));

        return View(secciones);
    }
}
