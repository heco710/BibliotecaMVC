# Verificación — 5 de septiembre de 2026

Rama local: `codex/semana-7-8`, creada desde `main` después de la integración del PR #3. No se publicó una nueva rama ni un PR.

## Resultado

- `dotnet build --no-restore`: correcto, 0 advertencias y 0 errores; SDK 10.0.302.
- Pruebas de repositorios, servicios e integración SQL: 35 comprobaciones correctas.
- Pruebas HTTP: 122 comprobaciones correctas, incluyendo CRUD de libros, autores y categorías, antiforgery, validación, 400/404, errores 503 y persistencia de categorías tras reiniciar la aplicación.
- Navegador: lista de categorías en escritorio y 390 × 844; formulario móvil, etiquetas y mensaje de nombre obligatorio visibles; botones utilizables y tabla contenida en la pantalla.
- Preparación de `BibliotecaDB` ejecutada dos veces: conserva exactamente las 3 categorías iniciales.

## Ambiente

SQL Server Express local `.\SQLEXPRESS`. Base de aplicación: `BibliotecaDB`. Base aislada de pruebas: `BibliotecaMVC_Test_20260905`.

La instancia estaba en modo de autenticación Windows, con otra conexión activa y otras bases existentes. Se utilizó autenticación Windows, sin cambiar el modo ni reiniciar SQL Server. La conexión de desarrollo se guardó en User Secrets, fuera del repositorio. Los scripts no modificaron Tickets ni BookReservation.

## Hallazgos corregidos durante las pruebas

- Los IDs de los POST de edición podían enlazarse desde el formulario tanto al argumento de ruta como al modelo, anulando el control de discordancia. Se añadió `[FromRoute]` en los tres módulos; también en la eliminación.
- La descripción de libro, visualmente opcional, estaba declarada no nullable. Se ajustó para permitir enviar el formulario sin descripción.
- Se corrigió el filtro de errores para identificar el controlador mediante `ControllerActionDescriptor`.

## Límites

La ruta opcional con login SQL no se ejecutó porque requeriría cambiar el modo de autenticación de la instancia. No se afirma una verificación de ese modo. Las comprobaciones del navegador fueron una revisión visual dirigida; no constituyen una auditoría completa de accesibilidad. EF Core y relaciones entre entidades quedan fuera de este hito, según el plan aprobado.
