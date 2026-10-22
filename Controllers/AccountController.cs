using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AccountController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var identifier = model.UsuarioOCorreo.Trim();
        var user = identifier.Contains('@')
            ? await users.FindByEmailAsync(identifier)
            : await users.FindByNameAsync(identifier);
        if (user is not null)
        {
            var result = await signIn.PasswordSignInAsync(user, model.Password, model.Recordarme, lockoutOnFailure: true);
            if (result.Succeeded) return Volver(model.ReturnUrl);
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente por demasiados intentos; espera 5 minutos y vuelve a intentarlo.");
                return View(model);
            }
        }

        ModelState.AddModelError(string.Empty, "Usuario, correo o contraseña incorrectos. Si agotaste los intentos, espera 5 minutos y vuelve a intentarlo.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) => View(new RegistroViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegistroViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new IdentityUser { UserName = model.Usuario.Trim(), Email = model.Email.Trim() };
        var result = await users.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Toda cuenta nueva empieza como Usuario; un Admin puede cambiar su rol en /Usuarios.
            var rol = await users.AddToRoleAsync(user, Roles.Usuario);
            if (!rol.Succeeded) throw new InvalidOperationException("No se pudo asignar el rol Usuario a la cuenta nueva.");
            await signIn.SignInAsync(user, isPersistent: false);
            return Volver(model.ReturnUrl);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // Destino de AccessDeniedPath: el usuario inició sesión, pero su rol no permite la operación.
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private IActionResult Volver(string? returnUrl) => Url.IsLocalUrl(returnUrl)
        ? LocalRedirect(returnUrl!)
        : RedirectToAction("Index", "Home");
}
