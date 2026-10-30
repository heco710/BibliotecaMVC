namespace BibliotecaMVC.Models;

// Datos de la partial _PaginaEstado, compartida por las páginas de error, acceso denegado y "no encontrado".
// Solo contiene textos pensados para el usuario final: nunca excepciones, rutas internas ni datos de conexión.
public sealed class PaginaEstadoViewModel
{
    public required string Codigo { get; init; }
    public required string Etiqueta { get; init; }
    public required string Titulo { get; init; }
    public required string Mensaje { get; init; }

    // Icono de la partial: "error", "candado", "busqueda" o "datos".
    public string Icono { get; init; } = "error";

    // Color de acento: "error", "aviso" o "info".
    public string Tono { get; init; } = "error";

    public string? Detalle { get; init; }
    public string? Referencia { get; init; }
    public IReadOnlyList<AccionEstado> Acciones { get; init; } = [];
    public bool MostrarSecciones { get; init; } = true;
}

public sealed record AccionEstado(string Texto, string Url, bool Principal = false);
