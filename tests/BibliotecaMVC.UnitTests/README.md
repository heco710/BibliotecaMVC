# Pruebas unitarias del módulo Libros (semana 16)

Proyecto xUnit vinculado a `BibliotecaMVC.slnx`. Referencia a `BibliotecaMVC.csproj` y usa **Moq** para simular las dependencias, por lo que no necesita SQL Server, archivos ni un usuario autenticado.

```powershell
dotnet test tests/BibliotecaMVC.UnitTests
```

En Visual Studio: abrir `BibliotecaMVC.slnx` y ejecutar desde **Prueba > Explorador de pruebas**.

## Componentes evaluados y límite de prueba

| Clase de prueba | Componente | Dependencias simuladas |
|---|---|---|
| `EntornoTests` | Configuración del proyecto | — |
| `LibroServiceTests` | `LibroService` (validación, ISBN único, portadas, CRUD) | `IRepositorioLibro` (EF Core) e `IImagenService` (disco) |
| `LibrosControllerTests` | `LibrosController` (resultados HTTP y mensajes) | `ILibroService`; logger con `NullLogger` |
| `ReporteLibrosServiceTests` | `ReporteLibrosService` y `ReportesController` (filtros, PDF y Excel; semana 17) | `IRepositorioLibro` e `IRepositorioAutor` |

El CRUD usa Entity Framework Core dentro de `RepositorioLibrosEf`. Las interfaces `IRepositorioLibro` e `ILibroService` ya existían y son el límite de prueba; no se modificó el código de la aplicación.

## Casos implementados (61 ejecuciones)

**Consulta**
- Listado con libros y listado vacío (`Assert.Equal`, `Assert.Empty`, `Verify` de una sola llamada).
- Libro por ID existente (`Assert.NotNull`) e inexistente (`Assert.Null`).
- Controlador: `Index` con y sin libros; `Details` existente y `NotFound` si no existe.

**Registro**
- Libro válido: se ignora el ID recibido y se asigna el generado por el repositorio.
- ISBN duplicado: error en `ISBN` y `AgregarAsync` nunca se invoca (`Times.Never`).
- `[Theory]` con 10 datos inválidos: título, autor, categoría o ISBN vacíos; título de 151 y ISBN de 21 caracteres; año menor a 1000 o futuro; descripción de 501 caracteres; imagen fuera del catálogo (`../../secret.txt`). Ninguno llega al repositorio.
- `[Theory]` de límites válidos: año 1000 y año actual; las cuatro portadas del catálogo.
- Archivo de imagen válido se guarda y se asigna; archivo inválido produce error en `imagenArchivo` sin guardar.
- Si la base de datos falla, la imagen subida se elimina para no dejar archivos huérfanos.
- Controlador: redirección a `Index` con mensaje, `ModelState` inválido no llama al servicio, error de ISBN visible en el campo.

**Edición**
- Libro inexistente devuelve "no encontrado" sin guardar.
- Datos válidos: el ISBN se verifica excluyendo el propio libro y la imagen actual no se borra.
- Se acepta conservar una imagen subida propia; se rechaza la imagen subida de otro libro.
- Cambiar a una portada del catálogo borra la imagen anterior.
- ISBN de otro libro y los 10 datos inválidos (`[Theory]`) se rechazan sin guardar.
- Si el libro se elimina mientras se edita, se borra la imagen nueva y se conserva la anterior.
- Controlador: `NotFound`, `BadRequest` si el ID de la ruta no coincide, redirección con mensaje.

**Eliminación**
- Libro existente: se eliminan el registro y su imagen.
- Libro inexistente o eliminación rechazada por la base: devuelve `false` y no se borra ninguna imagen.
- Controlador: `NotFound` en la confirmación y en el POST; redirección con mensaje si se elimina.

## Resultados y errores identificados

- Resultado final: **70 de 70 aprobadas** (61 del módulo Libros y 9 de reportería) — ver [`Evidencia/resultado-pruebas.txt`](Evidencia/resultado-pruebas.txt).
- La primera ejecución tuvo 1 falla en `AgregarAsync_ArchivoDeImagenValido...`: la prueba creaba el libro con `Imagen = ""`. `Imagen` es obligatoria y se valida antes de guardar el archivo. El formulario siempre envía una portada del `select` y el archivo la reemplaza, así que el error estaba en los datos de la prueba, no en la aplicación. Se corrigió la prueba.
- Prueba que falla intencionalmente: se cambió temporalmente el ID esperado de 25 a 26. xUnit informó `Expected: 26, Actual: 25` con el nombre de la prueba y la línea ([`Evidencia/prueba-fallida-intencional.txt`](Evidencia/prueba-fallida-intencional.txt)); se restauró el valor correcto.
- No se encontraron defectos en la lógica del módulo Libros con los escenarios evaluados. Una prueba aprobada no garantiza la ausencia de errores en toda la aplicación.

**No cubierto por pruebas unitarias:** el índice único de ISBN y las consultas reales de EF Core (`tests/BibliotecaMVC.Tests` con `BIBLIOTECA_TEST_CONNECTION`); la autorización por rol `[Authorize]` y el antiforgery, que dependen del pipeline de ASP.NET Core (`tests/http_smoke.py`); y la validación de firma de archivos de `ImagenService`, que ya se comprueba en `tests/BibliotecaMVC.Tests`.

## Niveles de prueba en el módulo Libros

| Nivel | Qué verifica | En este proyecto |
|---|---|---|
| Unitaria | Un método de forma aislada, con dependencias simuladas | `LibroService.AgregarAsync` rechaza un ISBN duplicado con `IRepositorioLibro` simulado |
| Integración | Controlador, servicio y base de datos juntos en un entorno de pruebas | `tests/BibliotecaMVC.Tests` aplica migraciones y ejecuta el CRUD de EF Core en una base `BibliotecaMVC_Test_*` |
| Extremo a extremo | Un flujo completo como usuario | `tests/http_smoke.py`: iniciar sesión como bibliotecario, consultar el catálogo y registrar un libro desde el formulario |

Las pruebas unitarias son rápidas y señalan el método exacto que falla, pero no detectan errores de configuración, SQL o permisos. Las de integración y de extremo a extremo los detectan, a cambio de ser más lentas y de necesitar una base de datos y la aplicación en ejecución.
