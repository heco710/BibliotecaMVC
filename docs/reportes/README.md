# Reportería del módulo Libros (semana 17)

`/Reportes` muestra una vista previa de los libros registrados en SQL Server, con filtros por autor y categoría, y descarga el mismo resultado como **ReporteLibros.pdf** (QuestPDF) o **ReporteLibros.xlsx** (ClosedXML). Pueden usarla los roles Usuario, Bibliotecario y Admin, igual que la consulta de Libros. El enlace **Reportes** aparece en el menú al iniciar sesión.

## Archivos

| Archivo | Responsabilidad |
|---|---|
| `Controllers/ReportesController.cs` | `Index` (vista previa), `LibrosPdf` y `LibrosExcel` (descargas con `File(...)`) |
| `Services/ReporteLibrosService.cs` | Consulta con los repositorios EF Core, filtros, resumen, PDF y Excel |
| `Services/Interfaces/IReporteLibrosService.cs` | Contrato del servicio, registrado en `Program.cs` |
| `Models/ReporteLibros.cs` | Datos del reporte: filas, filtros, fecha y totales |
| `Views/Reportes/Index.cshtml` | Filtros, resumen, botones **Generar reporte PDF** y **Exportar a Excel**, vista previa |

Se sigue el patrón del proyecto: controlador → servicio → repositorio. Los libros se obtienen con `IRepositorioLibro` (`RepositorioLibrosEf`, Entity Framework Core). En este proyecto, `Autor` y `Categoria` del libro son columnas de texto, no claves foráneas. Por eso el dato relacionado de autores (la **nacionalidad**) se obtiene de `IRepositorioAutor` (EF Core) cuando el nombre completo coincide; si no coincide, se muestra «—». La categoría es la del propio libro.

Los filtros viajan en la URL (`/Reportes/LibrosPdf?autor=...&categoria=...`). La vista previa, el PDF y el Excel llaman al mismo `ObtenerAsync`, así que siempre muestran los mismos registros.

## QuestPDF

Librería .NET de código abierto para crear PDF con una API fluida en C#, sin plantillas HTML ni conversores externos. El documento se describe por secciones (`Header`, `Content`, `Footer`) y componentes (`Column`, `Row`, `Table`, `Text`). La librería se encarga de los saltos de página y repite el encabezado de las tablas en cada página.

- Instalación: `dotnet add package QuestPDF` (versión 2026.9.1).
- Licencia: `QuestPDF.Settings.License = LicenseType.Community;`, en el constructor estático de `ReporteLibrosService`. La licencia Community es gratuita para proyectos educativos y organizaciones con ingresos anuales menores a 1 millón USD. Sin esta línea, QuestPDF lanza una excepción al generar el documento.
- `Document.Create(...).GeneratePdf()` devuelve un `byte[]` que el controlador envía con `File(bytes, "application/pdf", "ReporteLibros.pdf")`.

Diseño del PDF: A4 horizontal, encabezado con el nombre del sistema, título, filtros aplicados y fecha de generación; cinco tarjetas de resumen (total, disponibles, prestados, autores y categorías); tabla con encabezado oscuro, filas alternadas, bordes finos e ID y año centrados; el estado «Prestado» va resaltado; pie con «Página X de Y». Los colores son los de `wwwroot/css/site.css`.

## ClosedXML

Librería .NET de código abierto que crea y lee archivos Excel `.xlsx` (Open XML) sin tener Microsoft Excel instalado. Trabaja con objetos de alto nivel: `XLWorkbook` (libro), `IXLWorksheet` (hoja), `Cell(fila, columna)` y `Range(...)`.

- Instalación: `dotnet add package ClosedXML` (versión 0.105.1).
- Crear libro y hoja: `new XLWorkbook()` y `Worksheets.Add("Reporte de Libros")`.
- Datos: `hoja.Cell(fila, columna).Value = ...`. Los números y las fechas se guardan con su tipo, no como texto.
- Formato: `Style.Font.SetBold()`, `Fill.SetBackgroundColor(...)`, `Alignment.SetHorizontal(...)`, `Border.SetInsideBorder(...)`, `DateFormat.Format`, y `Columns().AdjustToContents()` para ajustar el ancho de las columnas.
- `libro.SaveAs(memoryStream)` genera el archivo en memoria para descargarlo.

Contenido del Excel: hoja **Reporte de Libros** con título combinado, fecha de generación (en formato de fecha), filtros aplicados y resumen. Debajo, la tabla con las columnas ID, Título, Autor, Nacionalidad, Categoría, Año, ISBN y Estado: encabezados en negrita, blancos sobre fondo oscuro y centrados; filas alternadas, bordes, ancho automático, encabezado fijo al desplazarse y autofiltro.

## Evidencias

Generadas con la aplicación ejecutándose sobre la base de pruebas `BibliotecaMVC_Test_Reportes_20261109` (catálogo inicial de 4 libros):

| Archivo | Contenido |
|---|---|
| `vista-reportes.jpg` | Vista sin filtros: 4 libros, 3 disponibles, 1 prestado |
| `vista-reportes-filtro-autor.jpg` | Filtro Autor = Jorge Luis Borges: 1 libro |
| `vista-reportes-movil.jpg` | Vista a 375 px, sin desplazamiento horizontal |
| `ReporteLibros-todos.pdf` / `.xlsx` | Reporte completo |
| `ReporteLibros-filtro-borges.pdf` / `.xlsx` | El mismo filtro en ambos formatos: 1 libro, «Autor: Jorge Luis Borges» |
| `ReporteLibros-sin-resultados.pdf` / `.xlsx` | Borges + Poesía: mensaje «No hay libros que coincidan…» |

Verificación del 9 de noviembre de 2026:

- 70 pruebas unitarias aprobadas, 9 de ellas de reportería (`tests/BibliotecaMVC.UnitTests/ReporteLibrosServiceTests.cs`). Comprueban los filtros, la nacionalidad, los totales, el PDF válido y el Excel: se vuelve a abrir con ClosedXML para revisar la hoja, los encabezados, la fecha, los filtros y las filas.
- 42 comprobaciones de C# y 330 HTTP aprobadas en la misma base de pruebas: el resto de la aplicación sigue funcionando.
- Descargas: HTTP 200, `Content-Type` correcto y nombre `ReporteLibros.pdf` / `ReporteLibros.xlsx`. El PDF se revisó visualmente; los datos coinciden con la tabla `Libros` de SQL Server.

## PDF frente a Excel

| | PDF (QuestPDF) | Excel (ClosedXML) |
|---|---|---|
| Uso principal | Presentar, imprimir y archivar | Analizar y reutilizar los datos |
| Diseño | Fijo: se ve igual en cualquier equipo | Depende del programa que lo abre |
| Edición | No editable | Se puede ordenar, filtrar, editar y calcular |
| Datos | Texto ya formateado | Celdas con tipo (números, fechas) y autofiltro |
| Paginación | Encabezado de tabla repetido y «Página X de Y» | Una hoja continua con el encabezado fijo |

El PDF sirve para entregar un documento oficial y el Excel, para trabajar con los datos. Por eso la aplicación ofrece ambos con los mismos filtros.
