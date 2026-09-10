# Contexto completo del proyecto BibliotecaMVC

> Documento de traspaso para continuar el proyecto en otro chat o sesión.
> Estado documentado: 16 de agosto de 2026.

## 1. Resumen ejecutivo

BibliotecaMVC es una aplicación académica ASP.NET Core MVC para una biblioteca virtual. El trabajo implementado combina tres etapas de ejercicios:

1. Razor y operaciones CRUD para autores y libros.
2. Rediseño de Home y About con HTML5, CSS3 y Bootstrap.
3. Organización adaptable con Flexbox, CSS Grid, componentes reutilizables y media queries.

El proyecto actualmente permite:

- listar, consultar, crear, editar y eliminar autores;
- listar, consultar, crear, editar y eliminar libros;
- asignar una imagen representativa a cada libro;
- indicar si un autor está activo y si un libro está disponible;
- navegar por una Home, un catálogo y una página About con diseño coherente;
- visualizar las páginas en escritorio, tableta y móvil.

La aplicación compila y funciona. Los datos de autores y libros se guardan en listas estáticas en memoria, siguiendo el material del curso. No existe base de datos. Al reiniciar la aplicación, las modificaciones hechas desde la interfaz regresan a los datos iniciales.

## 2. Ubicaciones importantes

- Carpeta del proyecto:
  `C:\Users\rodri\source\repos\BibliotecaMVC\BibliotecaMVC`
- Solución original:
  `C:\Users\rodri\source\repos\BibliotecaMVC\BibliotecaMVC.slnx`
- Repositorio GitHub:
  `https://github.com/heco710/BibliotecaMVC`
- Rama de trabajo publicada:
  `maintenance/gitignore`
- Commit de la implementación:
  `6f35ea9` — `Implementar catálogo CRUD y diseño responsivo`
- Pull request borrador:
  `https://github.com/heco710/BibliotecaMVC/pull/3`
- Rama de destino del PR:
  `main`
- Paquete de entrega local:
  `C:\Users\rodri\source\repos\BibliotecaMVC\BibliotecaMVC\BibliotecaMVC_Entrega.zip`

El PR #3 estaba abierto como borrador al crear este documento. Antes de realizar trabajo nuevo, comprobar en GitHub si ya fue integrado.

## 3. Tecnología y requisitos

- ASP.NET Core MVC.
- Target framework: `net10.0`.
- Nullable reference types habilitados.
- Implicit usings habilitados.
- Razor Views.
- Bootstrap incluido localmente en `wwwroot/lib/bootstrap`.
- jQuery y validación no intrusiva incluidos localmente.
- CSS personalizado en `wwwroot/css/site.css`.

Requisito para ejecutar el código:

- .NET 10 SDK. En la máquina donde se desarrolló se utilizó un SDK .NET 10 preview.

No hay paquetes NuGet adicionales declarados en el `.csproj`.

## 4. Cómo abrir y ejecutar

### Opción A: Visual Studio

1. Abrir `BibliotecaMVC.slnx`.
2. Esperar a que Visual Studio restaure y compile el proyecto.
3. Presionar F5 o usar el perfil `http`.

### Opción B: terminal

Desde la carpeta que contiene `BibliotecaMVC.csproj`:

```powershell
dotnet build
dotnet run --launch-profile http
```

La aplicación usa normalmente:

```text
http://localhost:5092
```

### Opción C: ZIP de entrega

1. Extraer completamente `BibliotecaMVC_Entrega.zip`.
2. Hacer doble clic en `Ejecutar.bat`.
3. El script verifica que `dotnet` exista y ejecuta el perfil HTTP.

No debe ejecutarse el archivo BAT directamente desde dentro del visor del ZIP; primero se debe extraer todo el contenido.

## 5. Configuración de inicio

`Program.cs` utiliza la configuración estándar de MVC:

- `AddControllersWithViews()`;
- `UseExceptionHandler()` fuera de Development;
- `UseHsts()` fuera de Development;
- `UseHttpsRedirection()`;
- `UseRouting()`;
- `UseAuthorization()`;
- `MapStaticAssets()`;
- ruta predeterminada `{controller=Home}/{action=Index}/{id?}`.

No hay autenticación, autorización por roles, Entity Framework ni servicios externos.

## 6. Estructura funcional

### 6.1 HomeController

Archivo: `Controllers/HomeController.cs`.

Contiene acciones simples para páginas informativas:

- `Index`;
- `Libros`;
- `Autores`;
- `Categorias`;
- `Acerca_de`;
- `Prestamos`;
- `Usuarios`;
- `Error`.

Importante: todavía existen las vistas antiguas `Views/Home/Libros.cshtml` y `Views/Home/Autores.cshtml`, pero la navegación principal ya utiliza `LibrosController` y `AutoresController`. No eliminar las vistas antiguas sin comprobar primero si el profesor espera que permanezcan.

### 6.2 AutoresController

Archivo: `Controllers/AutoresController.cs`.

Responsabilidades:

- mantener una lista estática inicial de autores;
- proteger lecturas y escrituras con un objeto `SyncRoot` y bloques `lock`;
- ordenar el listado por apellido;
- mostrar detalles por ID;
- crear autores;
- editar autores existentes;
- mostrar confirmación antes de eliminar;
- eliminar mediante POST;
- validar que la fecha de nacimiento no esté en el futuro;
- usar antiforgery tokens en POST;
- mostrar mensajes de éxito mediante `TempData`.

Acciones públicas:

| Método | Ruta habitual | Función |
|---|---|---|
| GET | `/Autores` | Listado |
| GET | `/Autores/Details/{id}` | Detalle |
| GET | `/Autores/Create` | Formulario de alta |
| POST | `/Autores/Create` | Crear |
| GET | `/Autores/Edit/{id}` | Formulario de edición |
| POST | `/Autores/Edit/{id}` | Actualizar |
| GET | `/Autores/Delete/{id}` | Confirmación |
| POST | `/Autores/Delete/{id}` | Eliminar |

Autores iniciales:

1. Gabriel García Márquez — Colombiana — inactivo.
2. Isabel Allende — Chilena — activa.
3. Jorge Luis Borges — Argentina — inactivo.
4. Laura Esquivel — Mexicana — activa.
5. Miguel Ángel Asturias — Guatemalteca — inactivo.

### 6.3 LibrosController

Archivo: `Controllers/LibrosController.cs`.

Implementa la misma estructura que Autores:

- lista estática y sincronización con `lock`;
- listado ordenado por título;
- detalle, alta, edición y eliminación;
- verificación de ID en los POST de edición;
- antiforgery tokens;
- mensajes de resultado con `TempData`;
- validación de año no futuro;
- validación de que la imagen elegida pertenezca al conjunto permitido.

Rutas:

| Método | Ruta habitual | Función |
|---|---|---|
| GET | `/Libros` | Catálogo |
| GET | `/Libros/Details/{id}` | Detalle |
| GET | `/Libros/Create` | Formulario de alta |
| POST | `/Libros/Create` | Crear |
| GET | `/Libros/Edit/{id}` | Formulario de edición |
| POST | `/Libros/Edit/{id}` | Actualizar |
| GET | `/Libros/Delete/{id}` | Confirmación |
| POST | `/Libros/Delete/{id}` | Eliminar |

Libros iniciales:

| ID | Título | Autor | Categoría | Año | Disponible | Imagen |
|---:|---|---|---|---:|---|---|
| 1 | Cien años de soledad | Gabriel García Márquez | Realismo mágico | 1967 | Sí | `cien-anos-soledad.png` |
| 2 | La casa de los espíritus | Isabel Allende | Narrativa | 1982 | Sí | `casa-espiritus.png` |
| 3 | Ficciones | Jorge Luis Borges | Cuentos | 1944 | No | `ficciones.png` |
| 4 | El señor Presidente | Miguel Ángel Asturias | Novela | 1946 | Sí | `senor-presidente.png` |

## 7. Modelos y validaciones

### 7.1 Autor

Archivo: `Models/Autor.cs`.

| Campo | Tipo | Reglas |
|---|---|---|
| `ID` | `int` | Identificador asignado en memoria |
| `Nombre` | `string` | Obligatorio, máximo 100 caracteres |
| `Apellido` | `string` | Obligatorio, máximo 100 caracteres |
| `Nacionalidad` | `string` | Obligatoria, máximo 50 caracteres |
| `FechaNacimiento` | `DateTime` | Obligatoria, formato fecha, no puede ser futura |
| `Activo` | `bool` | Estado del autor |

### 7.2 Libro

Archivo: `Models/Libro.cs`.

| Campo | Tipo | Reglas |
|---|---|---|
| `ID` | `int` | Identificador asignado en memoria |
| `Titulo` | `string` | Obligatorio, máximo 150 caracteres |
| `Autor` | `string` | Obligatorio, máximo 120 caracteres |
| `Categoria` | `string` | Obligatoria, máximo 80 caracteres |
| `AnioPublicacion` | `int` | Rango 1000–2100 y no puede superar el año actual |
| `ISBN` | `string` | Obligatorio, máximo 20 caracteres |
| `Descripcion` | `string` | Máximo 500 caracteres |
| `Imagen` | `string` | Obligatoria y limitada al catálogo de imágenes permitido |
| `Disponible` | `bool` | Disponibilidad del ejemplar |

Las validaciones DataAnnotations se muestran tanto en cliente como en servidor mediante tag helpers y `_ValidationScriptsPartial.cshtml`.

## 8. Vistas

### 8.1 Autores

Carpeta: `Views/Autores`.

- `Index.cshtml`: tabla adaptable con avatar de iniciales, estado y acciones.
- `Details.cshtml`: ficha completa del autor.
- `Create.cshtml`: alta.
- `Edit.cshtml`: edición.
- `Delete.cshtml`: confirmación de eliminación.
- `_Form.cshtml`: campos reutilizables compartidos entre alta y edición.

### 8.2 Libros

Carpeta: `Views/Libros`.

- `Index.cshtml`: catálogo con CSS Grid, imágenes, badges y acciones.
- `Details.cshtml`: imagen grande y ficha bibliográfica.
- `Create.cshtml`: alta.
- `Edit.cshtml`: edición.
- `Delete.cshtml`: confirmación con vista previa.
- `_Form.cshtml`: formulario reutilizable.

La selección de imagen utiliza cuatro opciones predefinidas. No existe carga de archivos por parte del usuario.

### 8.3 Home

Archivo principal: `Views/Home/Index.cshtml`.

Incluye:

- hero responsivo;
- botones hacia el catálogo y About;
- métricas visuales;
- carrusel Bootstrap;
- accesos rápidos;
- sección de libros destacados;
- sección de servicios;
- enlaces directos a detalles reales de libros.

### 8.4 About

Archivo: `Views/Home/Acerca_de.cshtml`.

Utiliza HTML5 semántico y estilos personalizados:

- hero;
- historia y datos de la biblioteca;
- misión, visión y valores;
- servicios reutilizables;
- llamada a la acción hacia el catálogo.

## 9. Layout, navegación y accesibilidad

Archivo: `Views/Shared/_Layout.cshtml`.

El layout contiene:

- idioma español;
- viewport adaptable;
- descripción meta;
- enlace “Saltar al contenido”;
- navbar Bootstrap colapsable;
- identidad “Biblioteca Virtual” con icono SVG;
- enlaces a Inicio, Categorías, Autores, Libros, Usuarios, Préstamos y About;
- footer con navegación secundaria;
- Bootstrap, jQuery y JavaScript del sitio.

La navegación de Libros apunta a `LibrosController/Index` y la de Autores a `AutoresController/Index`.

Se añadieron estados de foco visibles, textos alternativos en imágenes, etiquetas en formularios, roles y descripciones ARIA donde aportan contexto.

## 10. Diseño visual y CSS

Archivo principal: `wwwroot/css/site.css`.

Sistema visual:

- azul marino como color principal;
- coral para acciones y énfasis;
- dorado como acento;
- fondos crema y blanco;
- Georgia para encabezados;
- Segoe UI/Arial para texto;
- bordes suaves, tarjetas redondeadas y sombras discretas.

Variables principales:

```css
--ink: #17243d;
--paper: #fbfaf6;
--navy: #13233f;
--coral: #df6b54;
--coral-dark: #c4513d;
--gold: #d8aa55;
```

El archivo utiliza:

- Flexbox en navegación, acciones, encabezados y secciones;
- CSS Grid en catálogos, contenido About y componentes;
- `clamp()` para escalado tipográfico y espaciado;
- media queries en 1200, 992, 768 y 480 px aproximadamente;
- `prefers-reduced-motion` para accesibilidad;
- componentes reutilizables para formularios, detalles, badges, tablas y tarjetas.

No convertir todo a estilos inline. Mantener la mayor parte del diseño personalizado en `site.css`.

## 11. Imágenes

Carpeta: `wwwroot/images`.

Archivos:

- `cien-anos-soledad.png`;
- `casa-espiritus.png`;
- `ficciones.png`;
- `senor-presidente.png`.

Son ilustraciones originales generadas para el proyecto, en formato vertical y sin texto ni logotipos. Conceptos visuales:

- realismo mágico latinoamericano;
- casa familiar con atmósfera espiritual;
- laberinto literario y biblioteca imposible;
- ciudad política de tono oscuro.

Las vistas construyen la URL como `~/images/{Imagen}`. Si se añaden imágenes nuevas, también se debe actualizar:

1. el `<select>` de `Views/Libros/_Form.cshtml`;
2. la lista `imagenesPermitidas` de `LibrosController.ValidarLibro`;
3. los archivos físicos de `wwwroot/images`.

## 12. Material académico revisado

La carpeta local `Material Adicional` contiene nueve PDF y tres videos sobre:

- HTML5 y CSS3, parte I;
- Bootstrap y diseño responsivo, parte II;
- interfaces dinámicas con Razor.

La captura correspondiente al 24–30 de julio exige:

1. editar autores;
2. eliminar autores;
3. crear el módulo Libros;
4. listar, ver, agregar, editar y eliminar libros;
5. mostrar imágenes guardadas en `wwwroot/images`.

La captura de semana 4 exige:

- Home rediseñada con Bootstrap, Grid, Cards y botones;
- About con HTML5 semántico y CSS en `site.css`;
- temática de biblioteca virtual;
- diseño responsivo y media queries;
- un componente Bootstrap adicional.

La captura de semana 5 exige:

- Flexbox en Home;
- una sección con CSS Grid;
- diseño adaptable;
- al menos dos elementos visuales reutilizables en About;
- preservar los estilos de la semana anterior.

Decisión importante: `Material Adicional` no se subió a GitHub ni se incluyó en el ZIP, porque contiene videos de varios GB y es material de estudio, no código ejecutable.

## 13. Pruebas realizadas

### Compilación

Comando final:

```powershell
dotnet build --no-restore
```

Resultado final del proyecto original:

- 0 errores;
- 0 advertencias.

El ZIP se extrajo en una carpeta separada y se compiló con restauración:

- restauración correcta;
- 0 errores;
- 0 advertencias.

### Rutas verificadas

Se verificaron estas rutas, entre otras:

- `/`;
- `/Home/Acerca_de`;
- `/Autores`;
- `/Autores/Details/1`;
- `/Autores/Create`;
- `/Autores/Edit/1`;
- `/Autores/Delete/1`;
- `/Libros`;
- `/Libros/Details/1`;
- `/Libros/Create`;
- `/Libros/Edit/1`;
- `/Libros/Delete/1`.

Todas respondieron con HTTP 200 durante las pruebas.

También se probaron:

- creación de autor;
- edición de autor;
- eliminación del autor temporal;
- creación de libro;
- edición de libro;
- cambio de disponibilidad;
- eliminación del libro temporal;
- mensajes de validación por campos obligatorios;
- actualización de los listados después de cada operación.

La copia extraída del ZIP se inició y respondió correctamente en Home, About, Autores, Libros y detalle de libro.

## 14. Estado de Git y GitHub

Repositorio remoto:

```text
origin  https://github.com/heco710/BibliotecaMVC.git
```

Estado publicado al documentar:

- rama local: `maintenance/gitignore`;
- seguimiento: `origin/maintenance/gitignore`;
- commit HEAD: `6f35ea9`;
- PR #3 abierto como borrador hacia `main`.

El ZIP y este documento fueron creados después del commit `6f35ea9`. Por tanto, no asumir que estén en GitHub. Ejecutar `git status -sb` antes de cualquier commit adicional.

No hacer `git add -A` sin revisar el estado. El ZIP es un entregable y normalmente no debe entrar al repositorio. `Material Adicional` tampoco debe subirse.

## 15. Contenido del ZIP de entrega

Nombre:

```text
BibliotecaMVC_Entrega.zip
```

Incluye:

- `BibliotecaMVC.slnx`;
- carpeta `BibliotecaMVC` con el código fuente;
- Controllers, Models, Views y `wwwroot`;
- imágenes del catálogo;
- configuración y perfiles de ejecución;
- `Ejecutar.bat`;
- `LEEME.txt`;
- este archivo `CONTEXTO_PROYECTO.md` después de regenerar el ZIP.

Excluye:

- `.git`;
- `.vs`;
- `bin`;
- `obj`;
- `Material Adicional`;
- PDF y videos;
- archivos temporales.

## 16. Limitaciones conocidas

1. Los datos solo viven en memoria.
2. No existe base de datos ni migraciones.
3. No existe autenticación ni control de permisos.
4. No se cargan imágenes desde formularios; se seleccionan archivos predefinidos.
5. No hay búsqueda, filtrado ni paginación real.
6. No existen pruebas automatizadas en un proyecto de test.
7. Categorías, Usuarios y Préstamos son páginas informativas, no módulos CRUD.
8. La URL `Home/Libros` todavía existe como página antigua, aunque la navegación usa `/Libros`.
9. Se requiere .NET 10 SDK; en entornos con solo .NET 8 o 9 no compilará sin cambiar el target framework.

## 17. Recomendaciones para continuar

Orden recomendado si se amplía el proyecto:

1. Confirmar o integrar el PR #3 en `main`.
2. Crear una rama nueva desde `main`; no seguir acumulando cambios en `maintenance/gitignore` después del merge.
3. Incorporar Entity Framework Core y una base de datos.
4. Crear relaciones reales entre Libro, Autor y Categoría.
5. Sustituir el campo de autor textual de Libro por `AutorId`.
6. Añadir carga segura de imágenes con validación de extensión, MIME y tamaño.
7. Implementar búsqueda, filtros y paginación.
8. Añadir pruebas unitarias y de integración.
9. Implementar autenticación si se requieren roles de bibliotecario y usuario.
10. Retirar las vistas antiguas de Home solo después de confirmar que ya no forman parte de la evaluación.

## 18. Reglas de continuidad para otro chat

Al retomar:

1. Leer este documento completo.
2. Ejecutar `git status -sb` y comprobar el PR #3 antes de editar.
3. No modificar ni eliminar `Material Adicional`.
4. Preservar los cambios existentes del usuario.
5. Mantener el idioma de interfaz en español.
6. Mantener la temática Biblioteca Virtual y la paleta actual.
7. Mantener validación de cliente y servidor.
8. Mantener POST + antiforgery para operaciones que cambian datos.
9. Compilar después de cada cambio importante.
10. Probar las rutas afectadas y el comportamiento adaptable.

## 19. Comandos de diagnóstico rápidos

Desde la carpeta del proyecto:

```powershell
git status -sb
dotnet build --no-restore
dotnet run --no-build --launch-profile http
```

Para localizar rutas o referencias antiguas a Libros:

```powershell
rg -n "Home.*Libros|asp-action=\"Libros\"" Controllers Views
```

Para listar los archivos principales:

```powershell
rg --files Controllers Models Views wwwroot
```

## 20. Criterio de finalización actual

La implementación solicitada por las capturas y el material adicional está completa. El proyecto puede entregarse en su estado actual. Cualquier trabajo posterior debe considerarse una ampliación, corrección de evaluación o paso hacia persistencia real.
