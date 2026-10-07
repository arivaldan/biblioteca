# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Propósito

App pedagógica de préstamo de libros (`Libro`, `Socio`, `Préstamo`) para aprender desarrollo
apoyado por IA. Será traspasada a otro desarrollador junior para que la continúe: prioriza
código simple y legible sobre soluciones elegantes o genéricas.

Antes de empezar una tarea, lee `docs/estado-actual.md`.

## Comandos

```bash
dotnet build                                    # compila toda la solución
dotnet test                                     # corre todos los tests
dotnet test --filter "FullyQualifiedName~Test1" # corre un solo test
dotnet run --project src/Biblioteca.Api         # corre la Api (http://localhost:5211)
dotnet run --project src/Biblioteca.Web         # corre el Web (http://localhost:5047)
```

La solución es `Biblioteca.slnx` (formato XML nuevo, no `.sln`).

La Web necesita la Api corriendo. Desde la terminal, `dotnet run` de cada proyecto en dos
terminales. En Visual Studio, cada desarrollador crea su propio perfil de arranque múltiple
(no se sube al repositorio): clic derecho en la solución → *Configurar proyectos de inicio…* →
*Varios proyectos de inicio* → Api y Web en *Iniciar*. Visual Studio lo guarda en
`biblioteca.slnLaunch.user`, que está en `.gitignore`. No crear un `Biblioteca.slnLaunch`
compartido.

**El build trata cualquier warning como error** (`TreatWarningsAsErrors` en
`Directory.Build.props`). Si aparece un warning, se corrige — no se suprime.

## Arquitectura

- **`Biblioteca.Api`**: Web API con controllers. Flujo de una request:
  `Controller → Service → DbContext (EF Core)`. La lógica de negocio vive en los Services,
  no en los controllers ni en el DbContext.
- **`Biblioteca.Web`**: MVC que consume la Api mediante un `HttpClient` tipado. No accede a la
  base de datos ni referencia EF Core directamente. Flujo: `Controller → <Entidad>ApiClient
  (ApiClients/) → Api`. Los clientes devuelven `RespuestaApi<T>` (misma idea que `Resultado<T>`
  en la Api) y la URL de la Api está en `appsettings.json` (`Api:UrlBase`).
  Librerías front (Bootstrap, jQuery, SweetAlert2, Bootstrap Icons) copiadas a `wwwroot/lib`,
  sin npm ni LibMan. JavaScript propio en `wwwroot/js/site.js`, sin jQuery.
- **`Biblioteca.Contracts`**: DTOs compartidos entre Api y Web. Sin dependencias hacia Api/Web
  (contrato puro).
- **`Biblioteca.Api.Tests`**: xUnit, referencia a `Biblioteca.Api` directamente.
- **`Biblioteca.Web.Tests`**: xUnit; prueba los clientes de la Api con `HttpMessageHandlerFalso`
  (sin red ni Api real).

**No usar** patrón Repository, MediatR, AutoMapper, ni Clean Architecture completa (capas de
dominio/aplicación/infraestructura separadas). La prioridad es la simplicidad, no el
desacoplamiento máximo.

### Configuración centralizada

`Directory.Build.props` (raíz) fija para los 4 proyectos: `TargetFramework=net10.0`,
`Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`. No repetir estas
propiedades en los `.csproj` individuales. `global.json` fija el SDK en `10.0.401`
(`rollForward: latestFeature`).

## Base de datos (aún no implementado)

- App: SQL Server LocalDB. Tests: SQLite en memoria.
- El proveedor se elige en un solo lugar (`Program.cs` + `appsettings`), nunca hardcodeado en
  otras capas.

## Convenciones de código

- Dominio en español (`Libro`, `RegistrarPrestamo`); términos técnicos en inglés (`Controller`,
  `Service`, `Dto`).
- Métodos cortos, nombres descriptivos; comentarios que expliquen el *por qué*, no el *qué*.
- Código legible para un junior: evitar trucos, reflexión o abstracciones innecesarias.
- Toda regla de negocio debe tener al menos un test.
- `.editorconfig`: 4 espacios en `.cs` (2 en `.csproj`/`.json`/`.props`), llaves en línea nueva
  (Allman), `var` solo cuando el tipo es evidente, llaves obligatorias incluso en bloques de
  una línea. Las reglas de estilo están en `suggestion`/`silent`, nunca `warning` — con
  `TreatWarningsAsErrors` activo, una regla de estilo en `warning` rompería el build.

## Flujo de trabajo

`spec en docs/specs` → plan (plan mode) → implementación en pasos pequeños → tests → revisión →
commit.

## Prohibido

- Hacer `git push` o modificar la configuración de git sin pedir confirmación.
- Guardar secretos en `appsettings*.json` (usar `dotnet user-secrets`).
- Agregar paquetes NuGet sin justificarlo.
