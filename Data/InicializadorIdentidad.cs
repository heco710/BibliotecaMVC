using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Data;

public static class InicializadorIdentidad
{
    private sealed record UsuarioInicial(string Usuario, string Email, string Password, string Rol);

    // Aplica migraciones pendientes, crea los roles, crea las cuentas de "UsuariosIniciales"
    // y asigna Usuario a los usuarios existentes que todavía no tienen rol.
    public static async Task InicializarAsync(IServiceProvider servicios, IConfiguration configuracion)
    {
        await servicios.GetRequiredService<BibliotecaContext>().Database.MigrateAsync();

        var roles = servicios.GetRequiredService<RoleManager<IdentityRole>>();
        var usuarios = servicios.GetRequiredService<UserManager<IdentityUser>>();
        // Renombrar en lugar de crear de nuevo conserva las asignaciones de AspNetUserRoles.
        // Si el rol nuevo ya existe (por ejemplo, insertado por SQL), se trasladan los usuarios y se elimina el anterior.
        foreach (var (anterior, actual) in Roles.Renombrados)
        {
            var rolAnterior = await roles.FindByNameAsync(anterior);
            if (rolAnterior is null) continue;
            if (!await roles.RoleExistsAsync(actual))
            {
                Exigir(await roles.SetRoleNameAsync(rolAnterior, actual), anterior);
                Exigir(await roles.UpdateAsync(rolAnterior), anterior);
                continue;
            }
            foreach (var usuario in await usuarios.GetUsersInRoleAsync(anterior))
            {
                if (!await usuarios.IsInRoleAsync(usuario, actual)) Exigir(await usuarios.AddToRoleAsync(usuario, actual), usuario.UserName);
                Exigir(await usuarios.RemoveFromRoleAsync(usuario, anterior), usuario.UserName);
            }
            Exigir(await roles.DeleteAsync(rolAnterior), anterior);
        }
        foreach (var rol in Roles.Lista)
        {
            if (!await roles.RoleExistsAsync(rol)) Exigir(await roles.CreateAsync(new IdentityRole(rol)), rol);
        }

        foreach (var cuenta in configuracion.GetSection("UsuariosIniciales").Get<UsuarioInicial[]>() ?? [])
        {
            if (!Roles.Lista.Contains(cuenta.Rol))
                throw new InvalidOperationException($"UsuariosIniciales: el rol '{cuenta.Rol}' no existe.");

            var usuario = await usuarios.FindByNameAsync(cuenta.Usuario);
            if (usuario is null)
            {
                usuario = new IdentityUser { UserName = cuenta.Usuario, Email = cuenta.Email };
                Exigir(await usuarios.CreateAsync(usuario, cuenta.Password), cuenta.Usuario);
            }
            if (!await usuarios.IsInRoleAsync(usuario, cuenta.Rol))
                Exigir(await usuarios.AddToRoleAsync(usuario, cuenta.Rol), cuenta.Usuario);
        }

        foreach (var usuario in await usuarios.Users.ToListAsync())
        {
            if ((await usuarios.GetRolesAsync(usuario)).Count == 0)
                Exigir(await usuarios.AddToRoleAsync(usuario, Roles.Usuario), usuario.UserName);
        }
    }

    private static void Exigir(IdentityResult resultado, string? origen)
    {
        if (!resultado.Succeeded)
            throw new InvalidOperationException($"Identity ({origen}): " + string.Join(" ", resultado.Errors.Select(e => e.Description)));
    }
}
