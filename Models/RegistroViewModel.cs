using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models;

public sealed class RegistroViewModel
{
    [Required(ErrorMessage = "Ingresa un nombre de usuario.")]
    [StringLength(64, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 64 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Usa letras sin acentos, números, puntos, guiones o guiones bajos.")]
    [Display(Name = "Nombre de usuario")]
    public string Usuario { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(256, ErrorMessage = "El correo no puede superar los 256 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingresa una contraseña.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirma tu contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarPassword { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
