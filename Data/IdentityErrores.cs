using Microsoft.AspNetCore.Identity;

namespace BibliotecaMVC.Data;

public sealed class IdentityErrores : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName) => new()
        { Code = nameof(DuplicateUserName), Description = "Este nombre de usuario ya está registrado." };
    public override IdentityError DuplicateEmail(string email) => new()
        { Code = nameof(DuplicateEmail), Description = "Este correo electrónico ya está registrado." };
    public override IdentityError InvalidUserName(string? userName) => new()
        { Code = nameof(InvalidUserName), Description = "El nombre de usuario contiene caracteres no permitidos." };
    public override IdentityError InvalidEmail(string? email) => new()
        { Code = nameof(InvalidEmail), Description = "El correo electrónico no es válido." };
    public override IdentityError PasswordTooShort(int length) => new()
        { Code = nameof(PasswordTooShort), Description = $"La contraseña debe tener al menos {length} caracteres." };
    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
        { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "La contraseña debe incluir un símbolo." };
    public override IdentityError PasswordRequiresDigit() => new()
        { Code = nameof(PasswordRequiresDigit), Description = "La contraseña debe incluir un número." };
    public override IdentityError PasswordRequiresLower() => new()
        { Code = nameof(PasswordRequiresLower), Description = "La contraseña debe incluir una letra minúscula." };
    public override IdentityError PasswordRequiresUpper() => new()
        { Code = nameof(PasswordRequiresUpper), Description = "La contraseña debe incluir una letra mayúscula." };
}
