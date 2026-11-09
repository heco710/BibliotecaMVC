using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services.Interfaces;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BibliotecaMVC.Services;

// Reportes del módulo Libros. Los datos salen de la base mediante los repositorios de EF Core;
// QuestPDF genera el PDF y ClosedXML el Excel, ambos en memoria (byte[]) para descargarlos desde el controlador.
public sealed class ReporteLibrosService(IRepositorioLibro libros, IRepositorioAutor autores) : IReporteLibrosService
{
    // Licencia Community de QuestPDF: gratuita para proyectos educativos y organizaciones con ingresos anuales
    // menores a 1 millón USD. Sin esta línea, QuestPDF lanza una excepción al generar el documento.
    static ReporteLibrosService() => QuestPDF.Settings.License = LicenseType.Community;

    // Colores del sitio (wwwroot/css/site.css) para que los documentos mantengan la identidad visual.
    private const string Tinta = "#292925", Gris = "#686861", Papel = "#f7f7f2", Linea = "#d9d9d1", Aviso = "#7a5c1e";

    private static readonly string[] Columnas = ["ID", "Título", "Autor", "Nacionalidad", "Categoría", "Año", "ISBN", "Estado"];

    public async Task<ReporteLibros> ObtenerAsync(string? autor, string? categoria)
    {
        autor = string.IsNullOrWhiteSpace(autor) ? null : autor.Trim();
        categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();
        var todos = await libros.ObtenerTodosAsync();
        var nacionalidades = (await autores.ObtenerTodosAsync())
            .GroupBy(item => $"{item.Nombre} {item.Apellido}", StringComparer.OrdinalIgnoreCase)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.First().Nacionalidad, StringComparer.OrdinalIgnoreCase);

        // honey: filtra en memoria sobre el catálogo completo; adecuado para cientos de libros. Si el catálogo
        // crece a miles, mover el filtro a IRepositorioLibro para que EF Core lo traduzca a WHERE.
        var filas = todos
            .Where(libro => (autor is null || string.Equals(libro.Autor, autor, StringComparison.OrdinalIgnoreCase))
                && (categoria is null || string.Equals(libro.Categoria, categoria, StringComparison.OrdinalIgnoreCase)))
            .Select(libro => new FilaReporteLibro(libro, nacionalidades.GetValueOrDefault(libro.Autor)))
            .ToList();

        return new ReporteLibros(filas, autor, categoria, DateTime.Now,
            Opciones(todos.Select(libro => libro.Autor)), Opciones(todos.Select(libro => libro.Categoria)));
    }

    private static List<string> Opciones(IEnumerable<string> valores) =>
        valores.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.CurrentCulture).ToList();

    private static string[] Valores(FilaReporteLibro fila) =>
    [
        fila.Libro.ID.ToString(), fila.Libro.Titulo, fila.Libro.Autor, fila.Nacionalidad ?? "—", fila.Libro.Categoria,
        fila.Libro.AnioPublicacion.ToString(), fila.Libro.ISBN, fila.Libro.Disponible ? "Disponible" : "Prestado"
    ];

    public byte[] GenerarPdf(ReporteLibros reporte) => Document.Create(documento => documento.Page(pagina =>
    {
        pagina.Size(PageSizes.A4.Landscape());
        pagina.Margin(32);
        pagina.PageColor(Colors.White);
        pagina.DefaultTextStyle(estilo => estilo.FontSize(9).FontColor(Tinta));

        pagina.Header().PaddingBottom(14).BorderBottom(1).BorderColor(Tinta).PaddingBottom(10).Row(fila =>
        {
            fila.RelativeItem().Column(columna =>
            {
                columna.Item().Text("BIBLIOTECAMVC").FontSize(8).LetterSpacing(0.15f).FontColor(Gris);
                columna.Item().Text("Reporte de Libros").FontSize(22).Bold();
                columna.Item().Text($"Filtros aplicados — {reporte.DescripcionFiltros}").FontColor(Gris);
            });
            fila.ConstantItem(170).AlignRight().AlignBottom().Column(columna =>
            {
                columna.Item().AlignRight().Text("Fecha de generación").FontSize(8).FontColor(Gris);
                columna.Item().AlignRight().Text(reporte.FechaGeneracion.ToString("dd/MM/yyyy HH:mm")).Bold();
            });
        });

        pagina.Content().Column(columna =>
        {
            columna.Spacing(14);
            columna.Item().Row(resumen =>
            {
                resumen.Spacing(8);
                foreach (var (titulo, valor) in new[] { ("Total de libros", reporte.Total), ("Disponibles", reporte.Disponibles),
                    ("Prestados", reporte.Prestados), ("Autores", reporte.TotalAutores), ("Categorías", reporte.TotalCategorias) })
                {
                    resumen.RelativeItem().Background(Papel).Border(0.5f).BorderColor(Linea).Padding(8).Column(dato =>
                    {
                        dato.Item().Text(titulo).FontSize(8).FontColor(Gris);
                        dato.Item().Text(valor.ToString()).FontSize(16).Bold();
                    });
                }
            });

            if (reporte.Total == 0)
            {
                columna.Item().Background(Papel).Padding(16).AlignCenter().Text("No hay libros que coincidan con los filtros aplicados.").FontColor(Aviso);
                return;
            }

            columna.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(definicion =>
                {
                    definicion.ConstantColumn(34);  // ID
                    definicion.RelativeColumn(3);   // Título
                    definicion.RelativeColumn(2.4f); // Autor
                    definicion.RelativeColumn(1.4f); // Nacionalidad
                    definicion.RelativeColumn(1.4f); // Categoría
                    definicion.ConstantColumn(40);  // Año
                    definicion.RelativeColumn(1.8f); // ISBN
                    definicion.ConstantColumn(62);  // Estado
                });
                tabla.Header(encabezado =>
                {
                    foreach (var titulo in Columnas)
                        encabezado.Cell().Background(Tinta).PaddingVertical(6).PaddingHorizontal(5).Text(titulo).FontColor(Colors.White).Bold();
                });
                for (var i = 0; i < reporte.Filas.Count; i++)
                {
                    var fondo = i % 2 == 0 ? "#ffffff" : Papel;
                    var valores = Valores(reporte.Filas[i]);
                    for (var c = 0; c < valores.Length; c++)
                    {
                        var celda = tabla.Cell().Background(fondo).BorderBottom(0.5f).BorderColor(Linea).PaddingVertical(5).PaddingHorizontal(5);
                        var texto = (c is 0 or 5 ? celda.AlignCenter() : celda).Text(valores[c]);
                        if (c == 7 && !reporte.Filas[i].Libro.Disponible) texto.FontColor(Aviso);
                    }
                }
            });
        });

        pagina.Footer().BorderTop(0.5f).BorderColor(Linea).PaddingTop(6).Row(fila =>
        {
            fila.RelativeItem().Text("BibliotecaMVC · Reporte de Libros").FontSize(8).FontColor(Gris);
            fila.RelativeItem().AlignRight().Text(texto =>
            {
                texto.DefaultTextStyle(estilo => estilo.FontSize(8).FontColor(Gris));
                texto.Span("Página ");
                texto.CurrentPageNumber();
                texto.Span(" de ");
                texto.TotalPages();
            });
        });
    })).GeneratePdf();

    public byte[] GenerarExcel(ReporteLibros reporte)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Reporte de Libros");
        var ultimaColumna = Columnas.Length;

        hoja.Cell(1, 1).Value = "BibliotecaMVC · Reporte de Libros";
        hoja.Range(1, 1, 1, ultimaColumna).Merge().Style.Font.SetBold().Font.SetFontSize(16).Font.SetFontColor(XLColor.FromHtml(Tinta));
        hoja.Cell(2, 1).Value = "Fecha de generación:";
        hoja.Cell(2, 3).Value = reporte.FechaGeneracion;
        hoja.Cell(2, 3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
        hoja.Cell(2, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        hoja.Cell(3, 1).Value = "Filtros aplicados:";
        hoja.Cell(3, 3).Value = reporte.DescripcionFiltros;
        hoja.Cell(4, 1).Value = "Resumen:";
        hoja.Cell(4, 3).Value = $"Total de libros: {reporte.Total} · Disponibles: {reporte.Disponibles} · Prestados: {reporte.Prestados} · " +
            $"Autores: {reporte.TotalAutores} · Categorías: {reporte.TotalCategorias}";
        hoja.Range(2, 1, 4, 1).Style.Font.SetBold().Font.SetFontColor(XLColor.FromHtml(Gris));

        const int filaEncabezado = 6;
        for (var c = 0; c < ultimaColumna; c++) hoja.Cell(filaEncabezado, c + 1).Value = Columnas[c];
        var encabezado = hoja.Range(filaEncabezado, 1, filaEncabezado, ultimaColumna);
        encabezado.Style.Font.SetBold().Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(XLColor.FromHtml(Tinta))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        hoja.Row(filaEncabezado).Height = 22;

        var fila = filaEncabezado;
        foreach (var registro in reporte.Filas)
        {
            fila++;
            var datos = registro.Libro;
            hoja.Cell(fila, 1).Value = datos.ID;
            hoja.Cell(fila, 2).Value = datos.Titulo;
            hoja.Cell(fila, 3).Value = datos.Autor;
            hoja.Cell(fila, 4).Value = registro.Nacionalidad ?? "—";
            hoja.Cell(fila, 5).Value = datos.Categoria;
            hoja.Cell(fila, 6).Value = datos.AnioPublicacion;
            hoja.Cell(fila, 7).Value = datos.ISBN;
            hoja.Cell(fila, 8).Value = datos.Disponible ? "Disponible" : "Prestado";
            if (!datos.Disponible) hoja.Cell(fila, 8).Style.Font.SetFontColor(XLColor.FromHtml(Aviso));
            if ((fila - filaEncabezado) % 2 == 0) hoja.Range(fila, 1, fila, ultimaColumna).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Papel));
        }

        if (reporte.Total == 0)
        {
            fila++;
            hoja.Cell(fila, 1).Value = "No hay libros que coincidan con los filtros aplicados.";
            hoja.Range(fila, 1, fila, ultimaColumna).Merge().Style.Font.SetItalic().Font.SetFontColor(XLColor.FromHtml(Aviso));
        }
        else
        {
            hoja.Range(filaEncabezado, 1, fila, ultimaColumna).SetAutoFilter();
        }

        var tabla = hoja.Range(filaEncabezado, 1, fila, ultimaColumna);
        tabla.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorderColor(XLColor.FromHtml(Tinta)).Border.SetInsideBorderColor(XLColor.FromHtml(Linea));
        hoja.Range(filaEncabezado + 1, 1, fila, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        hoja.Range(filaEncabezado + 1, 6, fila, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Ancho según el contenido de la tabla (no el título combinado), con un mínimo legible.
        hoja.Columns(1, ultimaColumna).AdjustToContents(filaEncabezado, fila);
        foreach (var columna in hoja.Columns(1, ultimaColumna)) columna.Width = Math.Max(columna.Width + 2, 8);
        hoja.SheetView.FreezeRows(filaEncabezado);
        hoja.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        hoja.PageSetup.FitToPages(1, 0);

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }
}
