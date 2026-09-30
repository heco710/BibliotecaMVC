using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu usuario o correo electrónico.")]
    [StringLength(256, ErrorMessage = "El usuario o correo no puede superar los 256 caracteres.")]
    [Display(Name = "Usuario o correo electrónico")]
    public string UsuarioOCorreo { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Display(Name = "Recordar mi sesión")]
    public bool Recordarme { get; set; }

    public string? ReturnUrl { get; set; }
}
