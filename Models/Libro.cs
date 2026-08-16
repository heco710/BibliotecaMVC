using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models;

public class Libro
{
    public int ID { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor es obligatorio.")]
    [StringLength(120, ErrorMessage = "El autor no puede superar los 120 caracteres.")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(80, ErrorMessage = "La categoría no puede superar los 80 caracteres.")]
    [Display(Name = "Categoría")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "El año de publicación es obligatorio.")]
    [Range(1000, 2100, ErrorMessage = "Ingrese un año de publicación válido.")]
    [Display(Name = "Año de publicación")]
    public int AnioPublicacion { get; set; }

    [Required(ErrorMessage = "El ISBN es obligatorio.")]
    [StringLength(20, ErrorMessage = "El ISBN no puede superar los 20 caracteres.")]
    public string ISBN { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione una imagen.")]
    public string Imagen { get; set; } = "cien-anos-soledad.png";

    public bool Disponible { get; set; }
}
