using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliotecaMVC.Controllers;

internal static class ErroresDeValidacion
{
    public static void AgregarErrores(this ModelStateDictionary estado, IEnumerable<ValidationResult> errores)
    {
        foreach (var error in errores)
        {
            foreach (var campo in error.MemberNames.DefaultIfEmpty(string.Empty))
            {
                estado.AddModelError(campo, error.ErrorMessage ?? "El valor no es válido.");
            }
        }
    }
}
