# BibliotecaMVC — semanas 7 y 8

En este proyecto trabajé el mantenimiento de categorías de BibliotecaMVC con ASP.NET Core MVC, ADO.NET y SQL Server. Completé las opciones de editar y eliminar, además de mostrar y agregar categorías, para contar con las cuatro operaciones CRUD.

Para editar una categoría, el formulario carga su nombre y descripción actuales. Al guardar, utilizo una consulta `UPDATE` con parámetros SQL. Para eliminar, primero muestro una pantalla de confirmación y, cuando se confirma, ejecuto un `DELETE` con el ID como parámetro. No utilicé Entity Framework Core para estas operaciones.

También conservé el CRUD de libros y autores. Esos dos módulos trabajan con datos en memoria; las categorías sí se guardan en SQL Server y se conservan al reiniciar la aplicación.

## Cómo ejecuto el proyecto

Para ejecutar el proyecto necesito .NET SDK 10.0.302 (o un parche posterior de esa misma banda), SQL Server Express y Windows PowerShell 5.1. Para conectarme a SQL Server uso `Microsoft.Data.SqlClient` 6.1.6.

Desde la carpeta donde está `BibliotecaMVC.csproj`, ejecuto:

```powershell
dotnet restore
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -SaveConnectionString
dotnet run --launch-profile http
```

Después abro [el módulo de categorías](http://localhost:5092/Categorias). En mi configuración local uso `.\SQLEXPRESS`, que corresponde a `localhost\SQLEXPRESS`, con autenticación de Windows y la base `BibliotecaDB`. La cuenta con la que ejecuto la preparación debe tener permiso para crear la base. La aplicación usa esa misma cuenta de Windows; esta opción no crea una cuenta SQL independiente con permisos reducidos.

Para revisar los datos en SSMS, me conecto a `.\SQLEXPRESS` con **Autenticación de Windows**, actualizo **Bases de datos** y abro **BibliotecaDB → Tablas → dbo.Categorias**.

Guardé la conexión fuera del repositorio, en User Secrets con el ID `BibliotecaMVC-semana-8`. ASP.NET la carga en el ambiente Development, así que no dejé contraseñas en `appsettings.json`. Para usar otro ambiente, debo configurar `ConnectionStrings__BibliotecaDB` mediante su configuración de secretos. Uso `TrustServerCertificate=True` únicamente para esta instancia de desarrollo; para un despliegue necesitaría un certificado de servidor válido.

## Cómo preparé la base de datos

La tabla `dbo.Categorias` tiene estos campos:

| Columna | Tipo | Restricción |
|---|---|---|
| ID | int | PK, IDENTITY(1,1) |
| Nombre | nvarchar(100) | Obligatoria |
| Descripcion | nvarchar(250) | Admite NULL |

Adapté el material de clase en `Database/BibliotecaDB.sql`. El script crea la base y la tabla si todavía no existen, e inserta Novela, Ciencia Ficción e Historia solamente cuando crea la tabla. Si lo ejecuto otra vez, conserva los cambios y las eliminaciones que ya haya realizado. Si encuentra una tabla con una estructura incompatible, muestra un error; no hace una migración automática, no ejecuta `DROP` ni restablece los datos.

Si necesito usar otra instancia, cambio el valor de `-Server` en este comando:

```powershell
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -Server '.\SQLEXPRESS' -SaveConnectionString
```

También dejé preparada la alternativa de autenticación SQL del material de clase en `Database/CreateUser.sql`. La ejecuta el script de preparación y requiere que el administrador haya habilitado el modo mixto. El script no cambia esa configuración ni reinicia servicios. Para crear un login nuevo y guardar su contraseña aleatoria en User Secrets, puedo ejecutar:

```powershell
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -UseSqlAuthentication -SaveConnectionString
```

Si `biblioteca_user` ya existe, proporciono su contraseña actual desde Windows PowerShell sin mostrarla en pantalla:

```powershell
.\Database\Setup-Database.ps1 -UseSqlAuthentication -AppPassword (Read-Host 'Contraseña actual de biblioteca_user' -AsSecureString) -SaveConnectionString
```

El script conserva la contraseña de un login existente y asigna los roles `db_datareader` y `db_datawriter` dentro de la base seleccionada, sin conceder `db_owner` ni roles de servidor. Dejé esta alternativa preparada, pero no la probé en esta máquina porque la instancia está configurada únicamente para autenticación de Windows.

## Cómo organicé el código

Separé el código en vistas, controladores, servicios y repositorios. En las vistas muestro los datos y formularios con modelos tipados. En los controladores manejo las solicitudes HTTP, `ModelState`, las redirecciones y los mensajes. Dejé las validaciones de negocio en los servicios y el acceso a los datos en los repositorios. Registré las dependencias en `Program.cs` usando interfaces.

| Módulo | Servicio | Repositorio | Duración del repositorio |
|---|---|---|---|
| Libros | ILibroService | IRepositorioLibro / RepositorioLibrosEnMemoria | Singleton |
| Autores | IAutorService | IRepositorioAutor / RepositorioAutoresEnMemoria | Singleton |
| Categorías | ICategoriaService | IRepositorioCategoria / RepositorioCategoriasSql | Scoped |

Registré todos los servicios como Scoped. En los repositorios en memoria sincronicé los accesos, generé IDs crecientes y devolví copias para evitar que una lectura modifique los datos almacenados. Para categorías utilicé operaciones asíncronas, consultas parametrizadas y conexiones que se cierran al terminar cada operación.

Agregué la validación antiforgery a los formularios de escritura y comparé el ID de la ruta con el del formulario al editar. Si el registro no existe, la aplicación responde con 404; si los IDs no coinciden, responde con 400.

Dejé la descripción de categoría como opcional: elimino los espacios sobrantes y guardo `NULL` cuando está vacía. También validé que las fechas de publicación y nacimiento no sean futuras y que las imágenes pertenezcan al catálogo. Si falla la conexión o la configuración de categorías, muestro una página con estado 503 sin exponer la conexión ni indicar que la operación se completó.

## Cómo verifiqué el funcionamiento

En la comprobación del 10 de septiembre de 2026, ejecuté el proyecto con la conexión local y probé el CRUD de categorías mediante solicitudes HTTP y consultas directas a SQL Server. Pasaron 20 comprobaciones:

- Agregué una categoría temporal y comprobé que apareciera en la base de datos y en el listado.
- Cambié su nombre y descripción, y verifiqué ambos valores directamente en SQL Server.
- Reinicié la aplicación y confirmé que la edición se conservaba.
- Abrí la pantalla de eliminación y comprobé que el registro siguiera existiendo antes de confirmar.
- Confirmé la eliminación y revisé que la categoría desapareciera de SQL Server y del listado.
- Comparé los datos antes y después: las categorías existentes se conservaron y la categoría temporal quedó eliminada.

La compilación terminó con 0 errores y 0 advertencias. También pasaron 27 comprobaciones de repositorios y servicios; en esa ejecución no se incluyeron las pruebas automáticas de integración SQL porque `BIBLIOTECA_TEST_CONNECTION` no estaba configurada. La prueba de 20 comprobaciones que describí arriba se hizo por separado con la conexión actual.

Dejé pruebas ejecutables sin un framework de pruebas adicional. Para repetirlas, uso:

```powershell
dotnet run --project tests/BibliotecaMVC.Tests
```

Para ejecutar las pruebas automáticas de integración SQL y HTTP del repositorio, preparo una base dedicada. Estos scripts requieren una base de prueba; no uso `BibliotecaDB` como destino:

```powershell
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -DatabaseName BibliotecaMVC_Test_Local
$env:BIBLIOTECA_TEST_CONNECTION = 'Server=.\SQLEXPRESS;Database=BibliotecaMVC_Test_Local;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=5'
dotnet run --project tests/BibliotecaMVC.Tests
dotnet build
python tests/http_smoke.py
```

El script `http_smoke.py` necesita Python 3. Inicia la aplicación en el puerto 5198, prueba el CRUD de los tres módulos y la detiene al terminar. Con él puedo comprobar validación, antiforgery, IDs, errores de base de datos y persistencia después de reiniciar la aplicación. La base de prueba queda disponible; si interrumpo la ejecución, pueden quedar filas de prueba en ella. Los tests SQL requieren que su nombre comience con `BibliotecaMVC_Test_`.

Conservé los resultados de las pruebas anteriores, del 5 de septiembre, en [el documento de verificación](docs/VERIFICACION_SEMANAS_7_8.md).

## Qué incluye mi entrega

En esta entrega completé el mantenimiento de categorías con ADO.NET y dejé Entity Framework Core para el siguiente hito. Libros y autores siguen trabajando en memoria y vuelven a sus datos iniciales al reiniciar el proceso. Los campos `Libro.Autor` y `Libro.Categoria` siguen siendo texto libre, por lo que eliminar un autor o una categoría no modifica los libros. Conservé las pantallas informativas de usuarios y préstamos, así como las cifras y la selección de libros estáticas de la portada.

Conservé `CONTEXTO_PROYECTO.md` como referencia de la entrega anterior, de agosto de 2026. Para consultar el estado actual, uso este README y los resultados de septiembre.

Incluí el código fuente, los scripts y las pruebas en `BibliotecaMVC_Entrega.zip`, sin videos, credenciales, `.git`, `bin` ni `obj`. Ese ZIP conserva la documentación de cuando lo preparé; esta versión del README está actualizada en el repositorio. Para ejecutar el proyecto desde el ZIP, primero preparo la base con los pasos anteriores y después ejecuto `Ejecutar.bat`.
