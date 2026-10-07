# Cómo funciona la aplicación: el flujo de los datos

Explicación de por dónde parte la aplicación, cómo viajan los datos hasta la base de datos y
cómo vuelven hasta la pantalla. Usa Libro como ejemplo; Socio y Préstamo siguen el mismo flujo.

## Vista general

La aplicación son **dos programas** que corren a la vez y se hablan por HTTP. Solo la Api toca
la base de datos:

```
Navegador ──HTTP──▶ Biblioteca.Web (MVC, :5047) ──HTTP/JSON──▶ Biblioteca.Api (:5211) ──EF Core──▶ SQL Server LocalDB
   ▲                       │                                          │
   └──── HTML ─────────────┘◀──────────── JSON ──────────────────────┘
```

Los dos usan los mismos DTOs de `Biblioteca.Contracts` (`LibroDto`, `CrearLibroDto`,
`ActualizarLibroDto`), así que están de acuerdo en la forma de los datos.

## 1. Por dónde parte: el arranque

Cada proyecto empieza en su `Program.cs`, que registra las piezas y arranca el servidor.

**`src/Biblioteca.Api/Program.cs`**

- `AddControllers()`: activa los controllers de la Api.
- `AddDbContext<BibliotecaDbContext>(UseSqlServer(...))`: elige SQL Server LocalDB, con la
  cadena de conexión de `appsettings.Development.json`. Es el único lugar donde se decide la
  base de datos.
- `AddSingleton(TimeProvider.System)`: el reloj, que se usa en la regla del año.
- `AddScoped<LibroService>()`: el servicio con las reglas de negocio.
- `MapControllers()`: conecta las rutas como `/api/libros`.

**`src/Biblioteca.Web/Program.cs`**

- `AddControllersWithViews()`: MVC con vistas Razor.
- `AddHttpClient<LibrosApiClient>(...)`: el cliente que llama a la Api, con la URL
  `Api:UrlBase = http://localhost:5211/` de `appsettings.json`.
- La ruta por defecto `{controller=Home}/{action=Index}/{id?}`: `/Libros` llama a
  `LibrosController.Index`.

Las dos apps usan **inyección de dependencias**: nadie hace `new LibroService(...)`.
ASP.NET Core crea cada pieza y se la pasa por el constructor a quien la necesita.

## 2. Flujo de lectura: abrir el listado de libros

El usuario abre `http://localhost:5047/Libros`.

**En la Web**

1. **`LibrosController.Index()`** (`src/Biblioteca.Web/Controllers/LibrosController.cs`) llama a
   `_librosApi.ListarAsync()`.
2. **`LibrosApiClient.ListarAsync()`** (`src/Biblioteca.Web/ApiClients/LibrosApiClient.cs`)
   envía `GET http://localhost:5211/api/libros`.

**Viaje por la red hacia la Api**

3. **`LibrosController.Listar()`** de la Api (`src/Biblioteca.Api/Controllers/LibrosController.cs`)
   no tiene lógica: llama a `_libroService.ListarAsync()`.
4. **`LibroService.ListarAsync()`** (`src/Biblioteca.Api/Libros/LibroService.cs`):

   ```csharp
   _contexto.Libros.AsNoTracking().OrderBy(l => l.Titulo).ToListAsync();
   ```

5. **Aquí llega a la base de datos.** EF Core traduce esa consulta a SQL, algo como
   `SELECT ... FROM Libros ORDER BY Titulo`, y la ejecuta en LocalDB. Cada fila vuelve como un
   objeto `Libro`, la entidad de `Entidades/Libro.cs`.
6. El servicio convierte cada `Libro` en `LibroDto` con `ADto()`. Lo hace para no exponer la
   entidad de la base fuera de la Api.

**Viaje de vuelta**

7. El controller responde `Ok(libros)`, y ASP.NET Core lo convierte en **JSON** con código
   **200**:

   ```json
   [{"id":5,"titulo":"Cien años de soledad","autor":"Gabriel García Márquez",...}]
   ```

8. En la Web, `LibrosApiClient` recibe el 200 y convierte el JSON en `List<LibroDto>` con
   `ReadFromJsonAsync`. Lo devuelve envuelto en `RespuestaApi<T>` con `Estado = Ok`.
9. `LibrosController.Index()` hace `return View(lista)`.
10. **Razor genera el HTML.** `Views/Libros/Index.cshtml` recorre la lista con `@foreach` y
    arma la tabla. Esa vista se mete dentro de `Views/Shared/_Layout.cshtml`, que pone el menú
    lateral, los CSS y los scripts.
11. El navegador recibe **HTML ya armado** y lo muestra. Después ejecuta
    `wwwroot/js/site.js` para el menú hamburguesa, los mensajes y los botones.

En resumen, cada pieza tiene un solo trabajo: el controller decide, el cliente habla HTTP, el
servicio aplica las reglas, el `DbContext` habla con la base y la vista arma el HTML.

## 3. Flujo de escritura: crear un libro (con el modal)

**Abrir el modal**

1. El usuario pulsa "Nuevo libro". Es un botón con `data-url-formulario="/Libros/Crear"`.
2. `site.js` hace `fetch("/Libros/Crear")`. La Web devuelve solo el formulario
   (`PartialView("_FormularioCrear")`), que incluye un **token antiforgery** oculto.
3. `site.js` mete ese HTML en el modal, activa la validación del navegador y abre el modal.

**Guardar**

4. El usuario pulsa Guardar. Primero valida **jQuery Validation, en el navegador**, con los
   `[Required]` y `[Range]` de `CrearLibroDto`. Si falta el título, ni siquiera se envía.
5. `site.js` envía `fetch POST /Libros/Crear` con los campos y el token.
6. **En la Web:**
   - ASP.NET Core arma un `CrearLibroDto` con los campos del formulario (*model binding*) y
     comprueba el token (`[ValidateAntiForgeryToken]`).
   - Si `ModelState` no es válido, devuelve el formulario con errores sin llamar a la Api.
   - Si es válido, llama a `LibrosApiClient.CrearAsync(dto)`, que envía `POST /api/libros` con
     el DTO en **JSON**.
7. **En la Api:**
   - `[ApiController]` vuelve a validar los DataAnnotations. Si fallan, responde 400
     automáticamente.
   - **`LibroService.CrearAsync()`** aplica las reglas de la spec, en este orden:
     1. **Limpia los datos:** recorta título y autor (`Trim`) y normaliza el ISBN (quita
        guiones y espacios, `x` → `X`).
     2. **Valida** con `Validar()`: largos, formato del ISBN y año, que no puede ser posterior
        al del reloj.
     3. **Consulta la base:** `AnyAsync(l => l.Isbn == isbn)` comprueba si el ISBN ya existe.
     4. **Guarda:** `_contexto.Libros.Add(libro)` + `SaveChangesAsync()`. Aquí EF genera el
        **`INSERT`** y la base asigna el `Id`.
   - El servicio devuelve un `Resultado<LibroDto>`, que dice cómo terminó: `Ok`, `Invalido`,
     `IsbnDuplicado` o `NoEncontrado`.
   - El controller lo traduce a HTTP: `Ok` → **201 Created** con el libro en JSON.
     `Invalido` → **400**, `IsbnDuplicado` → **409**. Los errores van en formato
     `ProblemDetails`.

**Vuelta a la Web**

8. `LibrosApiClient` traduce el código HTTP a `RespuestaApi`: 201 → `Ok`, 400 → `Invalido` con
   los errores por campo, 409 → `IsbnDuplicado` con el error en `Isbn`, y si la Api no
   responde → `ApiNoDisponible`.
9. `LibrosController.Crear` decide qué contestar al navegador:
   - **Ok:** guarda "Se creó el libro…" en `TempData` y responde **200**.
   - **Errores:** los copia a `ModelState`, para que cada uno aparezca junto a su campo, y
     responde **400** con el formulario.
10. **En el navegador**, `site.js` mira el código:
    - **400:** reemplaza el contenido del modal. El usuario ve los errores y conserva lo que
      escribió.
    - **200:** recarga la página. Eso vuelve a ejecutar el **flujo de lectura** (sección 2), y
      la tabla ya incluye el libro nuevo.
11. Al recargar, `_Layout.cshtml` encuentra el mensaje en `TempData` y lo escribe en un `div`
    oculto. `site.js` lo lee y lo muestra con **SweetAlert2**.

Editar sigue el mismo camino, con `PUT /api/libros/{id}` y `UPDATE` en la base. Eliminar no
usa modal: `site.js` pide confirmación con SweetAlert2, la Web envía un POST normal, la Api
responde `204` y la base ejecuta un `DELETE`.

## 4. Dónde vive cada regla

| Qué | Dónde | Por qué ahí |
|---|---|---|
| Campo obligatorio, rango 1–100 | DataAnnotations en `Contracts` | Se comprueba tres veces: navegador, Web y Api. Es barato y da respuesta inmediata. |
| Trim, ISBN, año y duplicados | `LibroService` (Api) | Son reglas de negocio. Viven en un solo lugar, el que tiene acceso a los datos. |
| ISBN único en la base | Índice único en `BibliotecaDbContext` | Red de seguridad si dos peticiones llegan a la vez. |
| Qué código HTTP corresponde a cada error | `RespuestaDeError` en el controller de la Api | La traducción a HTTP está en un solo sitio. |
| Qué significa cada código HTTP | `EnviarAsync` en `LibrosApiClient` (Web) | La Web traduce HTTP a algo que entiende su controller. |
| Cómo se muestra al usuario | Vistas Razor + `site.js` | Es presentación, no lógica. |

## 5. Para verlo en vivo

- **Swagger** (`http://localhost:5211/swagger`): llama a la Api directamente, sin la Web, y
  muestra el JSON tal cual.
- **Puntos de interrupción en Visual Studio:** pon uno en `LibrosController.Index` de la Web y
  otro en `LibroService.ListarAsync` de la Api, arranca los dos con F5 y abre el listado. Verás
  saltar primero uno y después el otro, y podrás inspeccionar los datos en cada paso.
- **Pestaña Red** (F12 en el navegador): ahí aparecen el `fetch` del modal y su código 200 o
  400.
