using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services.Interfaces;

public interface IReporteLibrosService
{
    /// <summary>Consulta los libros en la base de datos y aplica los filtros; null o vacío significa "todos".</summary>
    Task<ReporteLibros> ObtenerAsync(string? autor, string? categoria);

    /// <summary>Documento PDF (QuestPDF) del reporte.</summary>
    byte[] GenerarPdf(ReporteLibros reporte);

    /// <summary>Libro de Excel .xlsx (ClosedXML) con la hoja "Reporte de Libros".</summary>
    byte[] GenerarExcel(ReporteLibros reporte);
}
