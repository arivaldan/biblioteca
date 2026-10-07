# Estado actual del proyecto

> Última actualización: 2026-10-07. Actualizar este archivo al terminar cada fase.

## Hecho

- Estructura de la solución, `CLAUDE.md` y `.claude/settings.json`.
- Libro API completa: spec en `docs/specs/libro.md`, migración `Inicial` y tests.
- Swagger UI en desarrollo (`/swagger`).
- **Web MVC de Libro:** spec en `docs/specs/libro-web.md`.
  - Listado, detalle, crear y editar (en modal) y eliminar, consumiendo la Api con `LibrosApiClient`
    (`HttpClient` tipado).
  - Menú lateral colapsable y portada con cards. Socios y Préstamos aparecen como
    "Próximamente".
  - SweetAlert2 (confirmar borrado y mensajes) y Bootstrap Icons, copiados a `wwwroot/lib`.
  - Tests de `LibrosApiClient` en `Biblioteca.Web.Tests`.
  - Arranque Api + Web: perfil local de Visual Studio de cada desarrollador (pasos en `CLAUDE.md`);
    no hay `.slnLaunch` compartido.

## Pendiente de Libro Web

- Prueba manual en el navegador de lo que solo se ve con JavaScript: diálogo de confirmación
  al eliminar (también cancelar), mensajes de éxito/error y menú hamburguesa. El resto se
  probó con la Api y la Web arrancadas.
- Prueba en el navegador de los modales de crear y editar: abrir, errores dentro del modal,
  guardar y recargar, y el card "Nuevo libro" de la portada (`/Libros#nuevo`).

## Próximas fases (en orden)

1. **Socio:** igual que Libro (Api + Web). Antes de empezar, crear una skill `/nueva-entidad`
   basada en lo aprendido con Libro. Lo aprendido en la Web:
   - Un cliente tipado por entidad (`ApiClients/<Entidad>ApiClient.cs`) que devuelve
     `RespuestaApi<T>`; reutilizar `RespuestaApi` y `HttpMessageHandlerFalso`.
   - Las claves de error de la Api coinciden con las propiedades de los DTOs: se copian tal
     cual a `ModelState`.
   - Un `int` obligatorio en un DTO necesita `[Required(ErrorMessage = ...)]` para que el
     mensaje de la validación en el navegador salga en español.
   - Activar Socios en el menú lateral y en la portada (quitar "Próximamente").
2. **Préstamo:** máximo 3 préstamos por socio, control de stock y devoluciones. Resolver las
   preguntas diferidas 14 y 15 de la spec de Libro. Completar el "Historial de préstamos" del
   detalle de libro.
3. **Cierre para el traspaso:** `README`, `setup.ps1`, subagente revisor de código y hook de
   `dotnet format`.

## Flujo de trabajo

rama → spec → plan mode → pasos pequeños con commit → PR → limpieza.
