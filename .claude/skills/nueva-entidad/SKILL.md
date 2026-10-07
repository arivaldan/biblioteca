---
name: nueva-entidad
description: Guía para agregar una entidad nueva a Biblioteca (Api + Web) siguiendo el molde de Libro - spec, plan por pasos, Contracts, entidad y migración, Service con Resultado<T>, controller, cliente tipado en la Web, vistas con modal, SweetAlert2 y tests. Usar cuando se pide crear Socio, Préstamo u otra entidad nueva.
argument-hint: <NombreEntidad en singular, ej. Socio>
---

# Nueva entidad: $ARGUMENTS

Libro es el **molde**: antes de escribir cada pieza, abre el archivo equivalente de Libro y
copia su forma (nombres, comentarios, orden de los métodos). No inventes otra estructura.
Si algo de Libro no encaja con la entidad nueva, plantéalo en el plan; no lo cambies por tu
cuenta.

En esta guía, `<Entidad>` es el singular (`Socio`) y `<Entidades>` el plural (`Socios`).

## 0. Antes de empezar

1. Lee `CLAUDE.md` y `docs/estado-actual.md`.
2. Igual que Libro, se hace en **dos fases**, cada una con su spec, su rama y su PR:
   - Fase Api: spec `docs/specs/<entidad>.md`, rama `feature/<entidad>-api`.
   - Fase Web: spec `docs/specs/<entidad>-web.md`, rama `feature/<entidad>-web`.

   Si el usuario prefiere hacerlo todo junto, que lo decida él.
3. Flujo de cada fase: **spec → plan mode → pasos pequeños → revisión → PR**. Al terminar
   cada paso: `dotnet build` (0 warnings), `dotnet test`, commit, resumen y **parar** hasta
   que el usuario diga que sigas. Nunca encadenar pasos ni hacer `git push` sin permiso.

## 1. Spec

- Copia `docs/specs/_plantilla.md`. Referencias: `docs/specs/libro.md` (Api) y
  `docs/specs/libro-web.md` (Web).
- Cada regla con ID (`RN-01`… en la Api, `RW-01`… en la Web) y al menos un test por regla.
- **No supongas respuestas.** Ponlas en "Preguntas abiertas" con tu recomendación y espera a
  que el usuario las conteste. Las típicas de una entidad:
  - Campos, cuáles son obligatorios, largos máximos y formatos.
  - ¿Hay algún campo único? (en Libro, el ISBN → 409 si se repite).
  - ¿Se normaliza o recorta algún texto antes de guardar?
  - Borrado físico o lógico; qué pasa si la entidad está relacionada con otra.
  - Orden del listado.
  - Relaciones con otras entidades (Préstamo une Libro y Socio).
- Cuando no quedan preguntas: estado `aprobada`, commit `docs: spec de <Entidad>`.

## 2. Fase Api

| Paso | Archivo nuevo o cambiado | Molde |
|------|--------------------------|-------|
| DTOs | `src/Biblioteca.Contracts/<Entidades>/Crear<Entidad>Dto.cs`, `Actualizar<Entidad>Dto.cs`, `<Entidad>Dto.cs` | `Contracts/Libros/` |
| Entidad | `src/Biblioteca.Api/Entidades/<Entidad>.cs` | `Entidades/Libro.cs` |
| Base de datos | `DbSet` + configuración en `Datos/BibliotecaDbContext.cs` | bloque de `Libro` en `OnModelCreating` |
| Migración | `dotnet ef migrations add <Nombre> --project src/Biblioteca.Api` | `Migrations/..._Inicial.cs` |
| Servicio | `src/Biblioteca.Api/<Entidades>/<Entidad>Service.cs` + `AddScoped` en `Program.cs` | `Libros/LibroService.cs` |
| Controller | `src/Biblioteca.Api/Controllers/<Entidades>Controller.cs` (ruta `api/<entidades>`) | `Controllers/LibrosController.cs` |
| Tests servicio | `tests/Biblioteca.Api.Tests/<Entidades>/<Entidad>ServiceTests.cs` | `LibroServiceTests.cs` + `BaseDeDatosEnMemoria` |
| Tests HTTP | `tests/Biblioteca.Api.Tests/<Entidades>/<Entidades>ControllerTests.cs` | `LibrosControllerTests.cs` + `BibliotecaApiFactory` |

Reglas de la Api:

- **DTOs:** sin `Id` en los de entrada. En ellos solo van las validaciones simples
  (DataAnnotations con `ErrorMessage` en español). Todo `int` obligatorio lleva
  `[Required(ErrorMessage = "...")]`: en la Api no hace nada, pero en la Web da el mensaje en
  español (sin él, MVC muestra "The X field is required.").
- **Servicio:** primero limpia los datos (trim, normalizar), después valida y por último va
  a la base. Devuelve `Resultado<T>` (`Libros/Resultado.cs`). Las claves del diccionario de
  errores son **los nombres de las propiedades del DTO** (`"Titulo"`, `"Isbn"`): la Web
  depende de eso para mostrar cada error junto a su campo. `AsNoTracking` al leer y
  `TimeProvider` si hay fechas (nunca `DateTime.Now`).
- **Controller:** sin lógica. Un único método `RespuestaDeError` con un `switch` que traduce el
  estado a HTTP; todos los errores con `ProblemDetails`; `CreatedAtAction` en el POST.
- **Migración:** revisa el archivo generado antes de seguir. `dotnet ef database update`
  pide permiso al usuario (está en `ask` de `.claude/settings.json`).
- **Para decidir en el plan:** `Resultado<T>` vive en el namespace `Biblioteca.Api.Libros` y su
  estado `IsbnDuplicado` es propio de Libro. Con una segunda entidad, propón moverlo a un lugar
  común y renombrar el 409 a algo genérico (por ejemplo, `Conflicto`) en vez de agregar un
  estado por entidad. Lo mismo para `RespuestaApi<T>` en la Web.

## 3. Fase Web

| Paso | Archivo nuevo o cambiado | Molde |
|------|--------------------------|-------|
| Cliente | `src/Biblioteca.Web/ApiClients/<Entidades>ApiClient.cs` + `AddHttpClient<...>` en `Program.cs` (reusar `urlBaseApi`) | `ApiClients/LibrosApiClient.cs` |
| Tests cliente | `tests/Biblioteca.Web.Tests/ApiClients/<Entidades>ApiClientTests.cs` | `LibrosApiClientTests.cs` + `HttpMessageHandlerFalso` |
| Controller | `src/Biblioteca.Web/Controllers/<Entidades>Controller.cs` | `Controllers/LibrosController.cs` |
| Listado y detalle | `Views/<Entidades>/Index.cshtml`, `Detalle.cshtml` | `Views/Libros/` |
| Formularios (modal) | `Views/<Entidades>/_FormularioCrear.cshtml`, `_FormularioEditar.cshtml` | `Views/Libros/_Formulario*.cshtml` |
| Menú y portada | `Views/Shared/_Layout.cshtml`, `Views/Home/Index.cshtml` | quitar "Próximamente" y enlazar |

Reglas de la Web:

- La Web **nunca** usa EF Core ni la base: todo pasa por el cliente tipado, que devuelve
  `RespuestaApi<T>`. El cliente traduce: 200/201/204 → `Ok`, 400 → `Invalido` (errores por
  campo), 404 → `NoEncontrado`, 409 → error en el campo único, Api apagada → `ApiNoDisponible`.
- **Crear y editar se hacen en un modal** (RW-09 de `libro-web.md`). Las acciones GET devuelven
  `PartialView`; las POST responden 200 (guardado, mensaje en `TempData`), 400 (formulario con
  errores, con `FormularioConErrores`) o 404/503 (mensaje en `TempData`). Los botones llevan
  `data-url-formulario`; las páginas incluyen `_ModalFormulario` y `_ValidationScriptsPartial`.
- **Eliminar:** formulario POST con clase `form-eliminar` y `data-titulo`; `site.js` pide la
  confirmación con SweetAlert2. Se vuelve al listado con el mensaje en `TempData`.
- Los textos de los formularios (etiquetas, ayudas) van en español en la vista, no en los DTOs.
- **Piezas de Libro que hay que generalizar** la primera vez que las use otra entidad
  (proponerlo en el plan):
  - `_ModalFormulario.cshtml` está en `Views/Libros/`: moverlo a `Views/Shared/`.
  - `site.js` dice "¿Eliminar este libro?" y busca el botón `boton-nuevo-libro` (para
    `#nuevo`): pasar el texto y el id por atributos `data-` o usar nombres genéricos.

## 4. Lecciones aprendidas con Libro

- Los mensajes del model binding ("abc" en un número, campo vacío) ya están en español para
  toda la Web (`Program.cs` de la Web): no hay que repetirlo por entidad.
- No agregar paquetes NuGet sin justificarlo. Las librerías front se copian a `wwwroot/lib`
  (sin npm ni LibMan).
- No crear un `Biblioteca.slnLaunch` compartido: cada desarrollador tiene su perfil local.
- `CLAUDE.md` tiene finales de línea CRLF: si lo editas con un script, respétalos.
- Al probar con `curl` en Windows, los acentos se codifican mal con `--data-urlencode`: envía
  el texto ya codificado en UTF-8 (`a%C3%B1o`). Un navegador no tiene ese problema.
- Pruebas manuales: `dotnet run --no-build --launch-profile http` de Api y Web, y detener los
  procesos al terminar. Borra los datos de prueba salvo que el usuario pida dejarlos.
- Lo que depende de JavaScript (modal, SweetAlert2, menú) no se puede comprobar con `curl`:
  dilo claramente y anótalo como pendiente, u ofrece probarlo en el navegador.

## 5. Cierre de cada fase

1. Actualiza `docs/estado-actual.md` (hecho, pendiente, próxima fase) y `CLAUDE.md` si
   apareció una convención nueva.
2. PR: `gh` no está instalado en el equipo del usuario. Haz `git push -u` de la rama (con
   permiso) y dale el enlace para crear la PR, con el título y la descripción listos para pegar.
3. Después del merge: `main` actualizada y la rama borrada (local con `git branch -d`; la
   remota solo si el usuario lo pide).
