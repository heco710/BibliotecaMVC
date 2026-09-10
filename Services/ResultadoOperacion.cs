using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Services;

public sealed record ResultadoOperacion(bool Encontrado, IReadOnlyList<ValidationResult> Errores)
{
    public bool Exitoso => Encontrado && Errores.Count == 0;

    public static List<ValidationResult> Validar(object modelo)
    {
        var errores = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), errores, validateAllProperties: true);
        return errores;
    }
}
