# BibliotecaMVC — semanas 7 y 8

Aplicación académica ASP.NET Core MVC. Libros y autores conservan su CRUD y sus datos iniciales en repositorios en memoria; categorías tiene CRUD completo persistente en SQL Server.

## Ejecutar

Requisitos: .NET SDK 10.0.302 (o parche posterior de esa banda), SQL Server Express y Windows PowerShell 5.1. El proyecto usa `Microsoft.Data.SqlClient` 6.1.6.

Desde la carpeta que contiene `BibliotecaMVC.csproj`:

```powershell
dotnet restore
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -SaveConnectionString
dotnet run --launch-profile http
```

Abrir http://localhost:5092/Categorias. El script usa `localhost\SQLEXPRESS` mediante el alias `.\SQLEXPRESS`, autenticación Windows y la base `BibliotecaDB`. La identidad que ejecuta la preparación necesita permiso para crear la base. La aplicación utiliza esa identidad Windows; este modo local no crea una cuenta SQL independiente con permisos reducidos.

En SSMS: conectar al servidor `.\SQLEXPRESS` con **Autenticación de Windows**, actualizar **Bases de datos** y abrir **BibliotecaDB → Tablas → dbo.Categorias**.

La conexión se guarda fuera del repositorio, en User Secrets con ID `BibliotecaMVC-semana-8`. ASP.NET la carga en Development. No hay contraseñas en `appsettings.json`. En otro ambiente, proporcionar `ConnectionStrings__BibliotecaDB` mediante su configuración de secretos. `TrustServerCertificate=True` es para esta instancia de desarrollo; un despliegue debe utilizar un certificado de servidor válido.

## Esquema y preparación

| Columna | Tipo | Restricción |
|---|---|---|
| ID | int | PK, IDENTITY(1,1) |
| Nombre | nvarchar(100) | Obligatoria |
| Descripcion | nvarchar(250) | Admite NULL |

`Database/BibliotecaDB.sql` adapta el material suministrado. Crea la base y tabla si faltan; inserta Novela, Ciencia Ficción e Historia únicamente al crear la tabla. Repetirlo conserva cambios y eliminaciones del usuario. Un esquema existente incompatible produce un error, sin migración automática. No ejecuta DROP ni restablece datos.

Para otra instancia:

```powershell
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -Server '.\SQLEXPRESS' -SaveConnectionString
```

El material también incluye un login SQL. La alternativa está implementada en `Database/CreateUser.sql`, ejecutado por el script de preparación. Requiere que el administrador haya habilitado modo mixto; el script no modifica ese ajuste ni reinicia servicios. Para crear un login nuevo, genera una contraseña aleatoria y la guarda en User Secrets:

```powershell
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -UseSqlAuthentication -SaveConnectionString
```

Si `biblioteca_user` ya existe, ejecutar desde Windows PowerShell y proporcionar su contraseña actual sin mostrarla:

```powershell
.\Database\Setup-Database.ps1 -UseSqlAuthentication -AppPassword (Read-Host 'Contraseña actual de biblioteca_user' -AsSecureString) -SaveConnectionString
```

No se cambia la contraseña de un login existente. Se asignan `db_datareader` y `db_datawriter` dentro de la base seleccionada, sin conceder `db_owner` ni roles de servidor. La ruta de autenticación SQL está preparada, pero no se probó en esta máquina porque la instancia permite únicamente autenticación Windows.

## Arquitectura

Las vistas reciben modelos tipados. Los controladores gestionan HTTP, ModelState, redirecciones y mensajes; dependen de interfaces de servicios. Los servicios validan las reglas de negocio y utilizan interfaces de repositorios. `Program.cs` registra las dependencias.

| Módulo | Servicio | Repositorio | Duración del repositorio |
|---|---|---|---|
| Libros | ILibroService | IRepositorioLibro / RepositorioLibrosEnMemoria | Singleton |
| Autores | IAutorService | IRepositorioAutor / RepositorioAutoresEnMemoria | Singleton |
| Categorías | ICategoriaService | IRepositorioCategoria / RepositorioCategoriasSql | Scoped |

Todos los servicios son Scoped. Los repositorios en memoria sincronizan accesos, generan IDs crecientes y devuelven copias para impedir cambios accidentales fuera de las operaciones de escritura. Las categorías usan operaciones asíncronas, SQL parametrizado y conexiones de corta duración. Los formularios de escritura validan antiforgery; las ediciones contrastan el ID de la ruta con el formulario. Las filas inexistentes responden 404 y los IDs discordantes, 400.

La descripción de categoría es opcional; se normalizan sus espacios y se guarda NULL cuando está vacía. Se rechazan publicaciones/nacimientos futuros e imágenes fuera del catálogo. Una falla de conexión o configuración en categorías muestra una página 503 sin exponer la conexión ni simular éxito.

## Verificar

Pruebas ejecutables sin un framework de pruebas adicional:

```powershell
dotnet run --project tests/BibliotecaMVC.Tests
```

Para incluir integración SQL y HTTP, crear una base dedicada (nunca usar `BibliotecaDB` como destino de las pruebas):

```powershell
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -DatabaseName BibliotecaMVC_Test_Local
$env:BIBLIOTECA_TEST_CONNECTION = 'Server=.\SQLEXPRESS;Database=BibliotecaMVC_Test_Local;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=5'
dotnet run --project tests/BibliotecaMVC.Tests
dotnet build
python tests/http_smoke.py
```

`http_smoke.py` necesita Python 3, inicia la aplicación en el puerto 5198, ejecuta CRUD de los tres módulos y la detiene al terminar. Comprueba validación, antiforgery, IDs, errores de base de datos y persistencia tras reiniciar la aplicación. La base de prueba permanece disponible; una ejecución interrumpida puede dejar filas de prueba allí. Los tests SQL requieren un nombre con prefijo `BibliotecaMVC_Test_`.

La evidencia de esta implementación está en `docs/VERIFICACION_SEMANAS_7_8.md`.

## Alcance y entrega

EF Core queda para el siguiente hito. Libros y autores vuelven a sus datos iniciales al reiniciar el proceso. `Libro.Autor` y `Libro.Categoria` siguen siendo texto libre; eliminar un autor o categoría no modifica libros. Usuarios y préstamos conservan sus pantallas informativas. La portada conserva sus cifras y selección de libros estáticas.

El documento `CONTEXTO_PROYECTO.md`, conservado de la entrega anterior, describe agosto de 2026; este README y la evidencia de septiembre documentan el estado actual. `BibliotecaMVC_Entrega.zip` contiene fuentes, scripts y pruebas; no incluye videos, credenciales, `.git`, `bin` ni `obj`. Al extraerlo, preparar la base siguiendo estas instrucciones antes de ejecutar `Ejecutar.bat`.
