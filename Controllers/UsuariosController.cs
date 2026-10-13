using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers;

// Solo el Administrador consulta y asigna los roles de los usuarios registrados.
[Authorize(Roles = Roles.Administrador)]
public sealed class UsuariosController(UserManager<IdentityUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var lista = new List<(IdentityUser Usuario, string Rol)>();
        foreach (var usuario in await users.Users.OrderBy(u => u.UserName).ToListAsync())
            lista.Add((usuario, (await users.GetRolesAsync(usuario)).FirstOrDefault() ?? "Sin rol"));
        return View(lista);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarRol(string id, string rol)
    {
        if (!Roles.Lista.Contains(rol)) return BadRequest();
        var usuario = await users.FindByIdAsync(id);
        if (usuario is null) return NotFound();
        if (usuario.Id == users.GetUserId(User))
        {
            TempData["Error"] = "No puedes cambiar tu propio rol; otro administrador debe hacerlo.";
            return RedirectToAction(nameof(Index));
        }

        // Cada usuario tiene un único rol: se quitan los anteriores y se asigna el nuevo.
        var actuales = await users.GetRolesAsync(usuario);
        var resultado = await users.RemoveFromRolesAsync(usuario, actuales);
        if (resultado.Succeeded) resultado = await users.AddToRoleAsync(usuario, rol);
        // Invalida la cookie del usuario: el nuevo rol se aplica al volver a iniciar sesión o, como máximo, en 30 minutos.
        if (resultado.Succeeded) resultado = await users.UpdateSecurityStampAsync(usuario);
        if (!resultado.Succeeded) throw new InvalidOperationException("No se pudo actualizar el rol del usuario.");

        TempData["Mensaje"] = $"{usuario.UserName} ahora tiene el rol {rol}.";
        return RedirectToAction(nameof(Index));
    }
}
