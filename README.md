# BibliotecaMVC

Aplicación ASP.NET Core MVC para administrar libros, autores y categorías. Libros y autores utilizan Entity Framework Core; categorías utiliza ADO.NET. Los datos se guardan en SQL Server.

## Requisitos

- .NET SDK 10.0.302 o un parche posterior de la misma banda.
- SQL Server Express y Windows PowerShell 5.1.
- Python 3 para las pruebas HTTP.

## Ejecución

Desde la carpeta del proyecto:

```powershell
dotnet restore
dotnet tool restore
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -SaveConnectionString
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update
dotnet run --launch-profile http
```

La aplicación está disponible en [localhost:5092](http://localhost:5092).

La configuración predeterminada utiliza `.\SQLEXPRESS`, la base `BibliotecaDB` y autenticación de Windows. El script prepara Categorias y guarda la conexión en User Secrets. Las migraciones incluidas crean Autores y Libros y cargan los datos iniciales una sola vez. Los cambios del catálogo se conservan al reiniciar.

Con la base preparada, basta con ejecutar `dotnet run --launch-profile http`. En la consola del Administrador de paquetes de Visual Studio, `Update-Database` aplica las migraciones pendientes.

## Funcionalidad

- Consulta, creación, edición y eliminación de libros, autores y categorías.
- Validación de campos, fechas e identificadores y protección antiforgery en formularios.
- Diseño adaptable, portadas editoriales y fotografías de autores almacenadas localmente.
- Fuentes y licencias de las imágenes en `/Home/Creditos`.

Los retratos se asignan por nombre completo en `Views/Autores/_Retrato.cshtml`. Los autores sin una fotografía asociada muestran «Sin foto».

Autor y Categoria del libro son campos de texto. Usuarios y préstamos están pendientes de implementación.

## Pruebas

Las comprobaciones de integración requieren una base separada:

```powershell
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -DatabaseName BibliotecaMVC_Test_EF_20260920
$env:BIBLIOTECA_TEST_CONNECTION = 'Server=.\SQLEXPRESS;Database=BibliotecaMVC_Test_EF_20260920;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=5'
dotnet run --project tests/BibliotecaMVC.Tests
python tests/http_smoke.py
```

La prueba de C# aplica las migraciones antes de comprobar el CRUD. Sin `BIBLIOTECA_TEST_CONNECTION`, solo ejecuta las validaciones y omite la integración SQL. Las pruebas HTTP inician su propia aplicación en el puerto 5198.
