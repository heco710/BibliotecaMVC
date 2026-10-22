# BibliotecaMVC

Aplicación ASP.NET Core MVC para administrar libros, autores y categorías, con registro, inicio de sesión y roles mediante ASP.NET Core Identity. Libros, autores y usuarios utilizan Entity Framework Core; categorías utiliza ADO.NET. Los datos se guardan en SQL Server.

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

La configuración predeterminada utiliza `.\SQLEXPRESS`, la base `BibliotecaDB` y autenticación de Windows. El script prepara Categorias y guarda la conexión en User Secrets. Las migraciones incluidas crean Autores, Libros y las tablas de Identity, y cargan los datos iniciales del catálogo una sola vez. Los cambios del catálogo y las cuentas se conservan al reiniciar.

Con la base preparada, basta con ejecutar `dotnet run --launch-profile http`: al iniciar, la aplicación aplica las migraciones pendientes, crea los roles y las cuentas de prueba. Si SQL Server no está disponible, arranca igual y los módulos con datos muestran la página de error controlada. En la consola del Administrador de paquetes de Visual Studio, `Update-Database` aplica las migraciones pendientes.

## Funcionalidad

- Libros: consulta para cualquier usuario con rol; agregar, editar y eliminar para Bibliotecario y Admin.
- Autores: consulta pública; agregar y editar para Bibliotecario y Admin; eliminar solo para Admin.
- Categorías: consulta pública; agregar, editar y eliminar para Bibliotecario y Admin.
- Portadas: una de las cuatro del catálogo o una imagen propia (JPG, PNG o WEBP, máximo 2 MB) guardada en `wwwroot/images/libros`.
- Confirmación con SweetAlert2 antes de guardar cambios en los formularios de edición.
- El ISBN de cada libro es único.
- Registro, Login por usuario o correo y cierre de sesión por POST con ASP.NET Core Identity.
- Validación de campos, fechas e identificadores y protección antiforgery en formularios.
- Diseño adaptable, portadas editoriales y fotografías de autores almacenadas localmente.
- Fuentes y licencias de las imágenes en `/Home/Creditos`.

Los retratos se asignan por nombre completo en `Views/Autores/_Retrato.cshtml`. Los autores sin una fotografía asociada muestran «Sin foto».

Autor y Categoria del libro son campos de texto. Préstamos está pendiente de implementación.

## Autenticación

1. Aplica la migración `AgregarIdentity` con `dotnet ef database update` (o `Update-Database` en Visual Studio). Solo añade tablas de Identity y conserva las tablas del catálogo.
2. Abre `/Account/Register` o pulsa **Registrarse**. Usa un nombre de usuario de 3 a 64 caracteres (letras sin acentos, números, puntos, guiones o guiones bajos) y un correo único.
3. La contraseña debe tener entre 8 y 100 caracteres, con mayúscula, minúscula, número y símbolo. El registro inicia sesión automáticamente; no requiere confirmación por correo.
4. Pulsa **Cerrar sesión**. En `/Account/Login`, vuelve a ingresar con el nombre de usuario o el correo y la contraseña.
5. **Recordar mi sesión** crea una cookie persistente con vigencia de 7 días, renovable por actividad. Sin esta opción, la cookie es de sesión. Tras 5 intentos fallidos, Identity bloquea la cuenta durante 5 minutos.

`UserManager` registra cuentas y `SignInManager` verifica contraseñas y gestiona la sesión. Identity almacena hashes de contraseña. Los formularios incluyen antiforgery y solo aceptan destinos locales para el retorno del Login. La interfaz reutiliza los colores, tipografía, campos y botones del catálogo.

## Autorización y roles

Autenticación identifica al usuario; autorización decide qué puede hacer. Haber iniciado sesión no da acceso a todas las operaciones.

| Rol | Libros | Autores | Categorías | Otros permisos |
|---|---|---|---|---|
| Usuario | Consultar | Consultar | Consultar | — |
| Bibliotecario | Consultar, agregar, editar y eliminar | Consultar, agregar y editar | Consultar, agregar, editar y eliminar | — |
| Admin | Consultar, agregar, editar y eliminar | Consultar, agregar, editar y eliminar | Consultar, agregar, editar y eliminar | Asignar roles en `/Usuarios` |

- `BibliotecaContext` hereda de `IdentityDbContext<IdentityUser>` y la migración `AgregarRoles` añade las tablas de roles.
- `LibrosController`, `AutoresController` y `CategoriasController` usan `[Authorize(Roles = ...)]` por acción; las constantes están en `Models/Roles.cs`.
- `RoleManager<IdentityRole>` crea los roles Admin, Bibliotecario y Usuario al iniciar si no existen. Los roles `Administrador` y `Lector` de versiones anteriores se renombran a `Admin` y `Usuario`, conservando las asignaciones. Si los roles nuevos ya existían (por ejemplo, insertados por SQL), los usuarios se trasladan y los roles anteriores se eliminan.
- Las vistas ocultan con `User.IsInRole(...)` los botones que el rol no puede usar, y el menú muestra **Roles** solo al Admin. Si se accede directamente a una URL restringida, se muestra `/Account/AccesoDenegado` (HTTP 403) con el rol actual y los permisos necesarios.
- Las cuentas nuevas reciben el rol Usuario mediante `UserManager<IdentityUser>`. Al iniciar, los usuarios existentes sin rol reciben Usuario.
- Un Admin cambia roles en `/Usuarios` (menú **Roles**). No puede cambiar su propio rol. El cambio se aplica cuando el usuario vuelve a iniciar sesión, o como máximo en 30 minutos.

Cuentas de prueba (solo en Development, definidas en `appsettings.Development.json`):

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin#2026` | Admin |
| `bibliotecario` | `Biblio#2026` | Bibliotecario |
| `usuario` | `Usuario#2026` | Usuario |

## Pruebas

Las comprobaciones de integración requieren una base separada:

```powershell
powershell.exe -NoProfile -File Database/Setup-Database.ps1 -DatabaseName BibliotecaMVC_Test_EF_20260920
$env:BIBLIOTECA_TEST_CONNECTION = 'Server=.\SQLEXPRESS;Database=BibliotecaMVC_Test_EF_20260920;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=5'
dotnet run --project tests/BibliotecaMVC.Tests
python tests/http_smoke.py
```

La prueba de C# aplica las migraciones antes de comprobar el CRUD. Sin `BIBLIOTECA_TEST_CONNECTION`, solo ejecuta las validaciones y omite la integración SQL. Las pruebas HTTP inician su propia aplicación en el puerto 5198 y comprueban registro, duplicados, Login por usuario/correo, cookies, cierre de sesión, bloqueo de cuenta, retorno local y persistencia, además del CRUD existente. La cuenta temporal de Identity se elimina al finalizar.

Si Windows impide ejecutar el script de preparación, usa `powershell.exe -NoProfile -ExecutionPolicy Bypass -File Database/Setup-Database.ps1` con los mismos argumentos; esta opción se limita al proceso que ejecuta el script.

Verificación realizada el 30 de septiembre de 2026: compilación sin advertencias ni errores, 37 comprobaciones de C# y 253 comprobaciones HTTP aprobadas en `BibliotecaMVC_Test_Identity_20260930`. La migración también se aplicó a `BibliotecaDB` y se compararon todas las filas del catálogo antes y después, sin cambios. Login y Registro se revisaron en navegador; a 390 px no presentan desbordamiento horizontal. Tras exigir sesión en el CRUD y añadir la migración `IndiceUnicoIsbn`: 38 comprobaciones de C# y 262 HTTP aprobadas en la misma base.

Verificación del 13 de octubre de 2026 (roles): 42 comprobaciones de C# y 311 HTTP aprobadas en `BibliotecaMVC_Test_Roles_20261013`, incluyendo acceso por rol, acceso directo denegado, asignación de roles y subida de portadas. Revisión en navegador con las cuentas Lector y Administrador.

Verificación del 22 de octubre de 2026 (semana 14): 42 comprobaciones de C# y 330 HTTP aprobadas en `BibliotecaMVC_Test_Roles_20261022`, incluyendo la matriz de permisos de Libros, Autores y Categorías por rol, acceso directo a URL restringidas, CRUD completo de Libros con la cuenta Bibliotecario y la migración de los roles anteriores sin duplicados (renombrado, o traslado cuando los roles nuevos ya se insertaron por SQL). Revisión en navegador con las cuentas usuario, bibliotecario y admin.

Referencia: [modelo de Identity y EF Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-10.0).
