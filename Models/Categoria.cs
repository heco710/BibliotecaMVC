using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models;

public class Categoria
{
    public int ID { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }
}
