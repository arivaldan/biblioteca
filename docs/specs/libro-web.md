# Spec: Libro en la Web (MVC)

> Estado: aprobada

## Objetivo

Permitir que el personal de la biblioteca gestione el catálogo de libros (listar, ver detalle,
crear, editar y eliminar) desde `Biblioteca.Web`, sin usar Swagger. La Web no tiene lógica de
negocio propia: consume la API de Libro (`docs/specs/libro.md`) mediante un `HttpClient`
tipado y muestra los errores que la API devuelve. También se crea la estructura general de la
Web (menú lateral y portada) que después usarán Socio y Préstamo.

## Fuera de alcance

- Cambios en la API o en las reglas de negocio de Libro (RN-01 a RN-09 siguen en `libro.md`).
- Paginación, búsqueda y filtros del listado.
- Autenticación y autorización.
- Pantallas de Socios y Préstamos (solo aparecen deshabilitadas en el menú y en la portada).
- Historial de préstamos en el detalle del libro: se deja el espacio preparado y se completa en
  la spec de Préstamo.
- Diseño visual propio: se usa el Bootstrap que ya trae la plantilla MVC.

## Modelo de datos

La Web no tiene modelo propio ni base de datos. Usa los DTOs de `Biblioteca.Contracts`:

| DTO                  | Uso en la Web                       |
|----------------------|-------------------------------------|
| `LibroDto`           | Listado y detalle                   |
| `CrearLibroDto`      | Formulario de alta                  |
| `ActualizarLibroDto` | Formulario de edición               |

Los campos y sus validaciones son los de `libro.md`. La Web **no** repite las reglas de
negocio (trim, ISBN, año, duplicados): las valida la API y la Web muestra el resultado.

## Reglas de la Web

Se usa el prefijo `RW` para no confundirlas con las reglas de negocio `RN` de `libro.md`.

1. **RW-01** — La Web nunca accede a la base de datos ni referencia EF Core. Todo pasa por
   el `HttpClient` tipado (`LibrosApiClient`).
2. **RW-02** — La URL base de la API se configura en `appsettings` (no hardcodeada en código).
3. **RW-03** — Si la API responde **400**, el formulario se vuelve a mostrar con los datos que
   escribió el usuario y cada error junto al campo que indica la API.
4. **RW-04** — Si la API responde **409** (ISBN duplicado), el formulario se vuelve a mostrar
   con los datos del usuario y el mensaje de la API junto al campo ISBN.
5. **RW-05** — Si la API responde **404** al ver, editar o eliminar un libro (por ejemplo, otro
   usuario ya lo borró), se vuelve al listado con un mensaje de error.
6. **RW-06** — Si la API no está disponible, se muestra un mensaje entendible dentro de la
   misma pantalla, en vez de una excepción.
7. **RW-07** — Antes de eliminar, el usuario confirma en un diálogo JavaScript (SweetAlert2).
   Si cancela, no se envía nada a la API.
8. **RW-08** — Tras crear, editar o eliminar con éxito se vuelve al listado y se muestra un
   mensaje de éxito con SweetAlert2. El mensaje viaja en `TempData` (patrón Post-Redirect-Get).

## Pantallas (rutas MVC)

| Método | Ruta                     | Qué hace                                       | Llama a la API          |
|--------|--------------------------|------------------------------------------------|-------------------------|
| GET    | /                        | Portada: bienvenida y cards de acceso directo  | —                       |
| GET    | /Libros                  | Listado ordenado por título (lo ordena la API) | GET /api/libros         |
| GET    | /Libros/Detalle/{id}     | Datos del libro + sección "Historial de préstamos" vacía | GET /api/libros/{id} |
| GET    | /Libros/Crear            | Formulario vacío                               | —                       |
| POST   | /Libros/Crear            | Envía el alta                                  | POST /api/libros        |
| GET    | /Libros/Editar/{id}      | Formulario con los datos actuales              | GET /api/libros/{id}    |
| POST   | /Libros/Editar/{id}      | Envía la edición                               | PUT /api/libros/{id}    |
| POST   | /Libros/Eliminar/{id}    | Borra el libro (tras confirmar con SweetAlert2) | DELETE /api/libros/{id} |

Los POST usan token antiforgery. El botón "Eliminar" está en el listado y en el detalle; es un
formulario POST cuyo envío intercepta JavaScript para pedir la confirmación.

### Estructura general (layout)

- **Menú lateral (sidebar)** a la izquierda con: Inicio, Libros, Socios y Préstamos, cada uno
  con su icono de Bootstrap Icons. Socios y Préstamos aparecen deshabilitados con la etiqueta
  "Próximamente".
- Un **botón hamburguesa** colapsa y despliega el menú. En pantallas pequeñas el menú empieza
  colapsado.
- Se eliminan la barra superior de la plantilla y la página `Privacy`.

### Portada

- Mensaje de bienvenida.
- Cards con icono y acceso directo: "Ver libros", "Nuevo libro", "Socios" y "Préstamos".
  Los de Socios y Préstamos aparecen deshabilitados con la etiqueta "Próximamente".

### Arranque

- `Biblioteca.slnLaunch` define un perfil de arranque múltiple (Api + Web) para Visual Studio.
- La Web llama a la Api en `http://localhost:5211` (configurado en `appsettings` según RW-02).

## Criterios de aceptación

**Estructura**

- [ ] La portada muestra la bienvenida y los cards; el card "Ver libros" lleva al listado.
- [ ] El menú lateral aparece en todas las páginas y el botón hamburguesa lo colapsa y despliega.
- [ ] Desde el menú lateral se llega al listado de libros.
- [ ] Socios y Préstamos aparecen deshabilitados ("Próximamente") en el menú y en la portada.
- [ ] Con el perfil de `Biblioteca.slnLaunch`, un solo F5 en Visual Studio arranca Api y Web.

**Libros**

- [ ] El listado muestra todos los libros con título, autor, ISBN, año y ejemplares.
- [ ] Con la API sin libros, el listado muestra "No hay libros registrados".
- [ ] El detalle muestra todos los datos del libro y la sección de historial de préstamos vacía.
- [ ] Se puede crear un libro válido, aparece en el listado y se ve el mensaje de éxito.
- [ ] Un campo vacío se marca en el navegador antes de enviar (validación cliente).
- [ ] Crear un libro con un dato que rechaza la API (ej. ISBN con formato inválido) muestra el
      error junto al campo y conserva lo escrito.
- [ ] Crear un libro con un ISBN ya registrado muestra el error junto al campo ISBN y conserva
      lo escrito.
- [ ] Se puede editar un libro, el cambio se ve en el listado y se ve el mensaje de éxito.
- [ ] Editar usando el ISBN de otro libro muestra el error de ISBN duplicado.
- [ ] Al pulsar "Eliminar" aparece la confirmación; si se cancela, el libro sigue; si se
      confirma, desaparece del listado y se ve el mensaje de éxito.
- [ ] Ver, editar o eliminar un libro que ya no existe vuelve al listado con un mensaje (RW-05).
- [ ] Con la API apagada, la Web muestra un mensaje entendible (RW-06).

**Tests (`Biblioteca.Web.Tests`)**

- [ ] `LibrosApiClient` se prueba con un `HttpMessageHandler` falso: respuestas 200/201/204,
      400 (errores por campo), 404, 409 y API no disponible.

## Decisiones

1. **Tests de la Web:** se crea `Biblioteca.Web.Tests` (xUnit) y se prueba `LibrosApiClient`
   con un `HttpMessageHandler` falso.
2. **Confirmación al eliminar:** con JavaScript (SweetAlert2), para mostrar también el uso de
   JavaScript en el proyecto. No hay página de confirmación.
3. **Pantalla de detalle:** sí; más adelante mostrará el historial de préstamos y a quién se
   prestó el libro.
4. **Validación en el navegador:** sí, con jQuery Validation y los DataAnnotations de los DTOs.
   Las reglas de negocio se siguen validando en la API.
5. **Dónde mostrar el 409:** junto al campo ISBN.
6. **404 al ver/editar/eliminar:** se vuelve al listado con un mensaje.
7. **API caída:** mensaje dentro de la misma pantalla.
8. **Mensajes de éxito:** con SweetAlert2, pasando el texto por `TempData`.
9. **Portada y menú:** menú lateral colapsable con botón hamburguesa (Inicio, Libros, Socios,
   Préstamos); portada con bienvenida y cards de acceso directo. Se elimina `Privacy`.
10. **Arrancar Api y Web juntas:** arranque múltiple de Visual Studio con un perfil guardado en
    `Biblioteca.slnLaunch` (un solo F5 arranca las dos). Desde la terminal se sigue pudiendo
    arrancar cada proyecto con `dotnet run`.
11. **Cómo se incluye SweetAlert2:** se copian sus archivos a `wwwroot/lib/sweetalert2`, como
    Bootstrap y jQuery (funciona sin internet, no es un paquete NuGet).
12. **Socios y Préstamos en el menú y la portada:** se muestran deshabilitados con la etiqueta
    "Próximamente" hasta que se implementen.
13. **Iconos:** se agrega Bootstrap Icons, copiado a `wwwroot/lib/bootstrap-icons`.

## Preguntas abiertas

(Ninguna.)
