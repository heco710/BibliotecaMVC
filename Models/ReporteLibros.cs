namespace BibliotecaMVC.Models;

// Datos de un reporte de libros: los mismos para la vista previa, el PDF y el Excel, así los filtros coinciden en los tres.
// Autor y Categoria del libro son texto; la nacionalidad se obtiene del módulo Autores cuando el nombre coincide.
public sealed record ReporteLibros(
    IReadOnlyList<FilaReporteLibro> Filas,
    string? Autor,
    string? Categoria,
    DateTime FechaGeneracion,
    IReadOnlyList<string> AutoresDisponibles,
    IReadOnlyList<string> CategoriasDisponibles)
{
    public int Total => Filas.Count;
    public int Disponibles => Filas.Count(fila => fila.Libro.Disponible);
    public int Prestados => Total - Disponibles;
    public int TotalAutores => Filas.Select(fila => fila.Libro.Autor).Distinct(StringComparer.OrdinalIgnoreCase).Count();
    public int TotalCategorias => Filas.Select(fila => fila.Libro.Categoria).Distinct(StringComparer.OrdinalIgnoreCase).Count();
    public string DescripcionFiltros => $"Autor: {Autor ?? "Todos"} · Categoría: {Categoria ?? "Todas"}";
}

public sealed record FilaReporteLibro(Libro Libro, string? Nacionalidad);
