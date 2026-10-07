# Estado actual del proyecto

> Última actualización: 2026-10-07. Actualizar este archivo al terminar cada fase.

## Hecho

- Estructura de la solución, `CLAUDE.md` y `.claude/settings.json`.
- Libro API completa: spec en `docs/specs/libro.md`, migración `Inicial` y tests.
- Swagger UI en desarrollo (`/swagger`).

## Próximas fases (en orden)

1. **Web MVC de Libro:** listado y formularios que consumen la API con un `HttpClient` tipado.
   Los formularios muestran los errores 400 y 409 que devuelve la API.
2. **Socio:** igual que Libro. Antes de empezar, crear una skill `/nueva-entidad` basada en lo
   aprendido con Libro.
3. **Préstamo:** máximo 3 préstamos por socio, control de stock y devoluciones. Resolver las
   preguntas diferidas 14 y 15 de la spec de Libro.
4. **Cierre para el traspaso:** `README`, `setup.ps1`, subagente revisor de código y hook de
   `dotnet format`.

## Flujo de trabajo

rama → spec → plan mode → pasos pequeños con commit → PR → limpieza.
