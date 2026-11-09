using BibliotecaMVC.Models;
using BibliotecaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

// Reportería del módulo Libros: vista previa con filtros y descarga en PDF (QuestPDF) y Excel (ClosedXML).
// Mismo acceso que la consulta de Libros: cualquier usuario con rol. Los filtros viajan en la URL (GET),
// así la vista previa, el PDF y el Excel usan exactamente los mismos criterios.
[Authorize(Roles = Roles.Todos)]
public class ReportesController(IReporteLibrosService servicio, ILogger<ReportesController> logger) : Controller
{
    public const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task<IActionResult> Index(string? autor, string? categoria)
    {
        logger.LogInformation("Vista de reportes solicitada por {Usuario} (autor: {Autor}, categoría: {Categoria}).", User.Identity?.Name, autor, categoria);
        return View(await servicio.ObtenerAsync(autor, categoria));
    }

    public async Task<IActionResult> LibrosPdf(string? autor, string? categoria)
    {
        var reporte = await servicio.ObtenerAsync(autor, categoria);
        logger.LogInformation("Reporte PDF de libros generado para {Usuario}: {Cantidad} registros ({Filtros}).", User.Identity?.Name, reporte.Total, reporte.DescripcionFiltros);
        return File(servicio.GenerarPdf(reporte), "application/pdf", "ReporteLibros.pdf");
    }

    public async Task<IActionResult> LibrosExcel(string? autor, string? categoria)
    {
        var reporte = await servicio.ObtenerAsync(autor, categoria);
        logger.LogInformation("Reporte Excel de libros generado para {Usuario}: {Cantidad} registros ({Filtros}).", User.Identity?.Name, reporte.Total, reporte.DescripcionFiltros);
        return File(servicio.GenerarExcel(reporte), TipoExcel, "ReporteLibros.xlsx");
    }
}
