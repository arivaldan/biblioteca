using Biblioteca.Contracts.Libros;
using Biblioteca.Web.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

/// <summary>
/// Pantallas de libros. No tiene lógica de negocio: llama a la Api mediante LibrosApiClient
/// y decide qué vista mostrar según la respuesta.
/// </summary>
public class LibrosController : Controller
{
    // Claves de TempData que lee _Layout.cshtml para mostrar los mensajes con SweetAlert2 (RW-08).
    private const string ClaveMensajeExito = "MensajeExito";
    private const string ClaveMensajeError = "MensajeError";

    private const string MensajeLibroNoEncontrado = "El libro ya no existe. Puede que otra persona lo haya eliminado.";
    private const string MensajeApiNoDisponible =
        "No se pudo conectar con el servidor de la biblioteca (Api). Tus datos no se guardaron; inténtalo de nuevo.";

    private readonly LibrosApiClient _librosApi;

    public LibrosController(LibrosApiClient librosApi)
    {
        _librosApi = librosApi;
    }

    // GET /Libros
    public async Task<IActionResult> Index()
    {
        RespuestaApi<List<LibroDto>> respuesta = await _librosApi.ListarAsync();
        if (respuesta.Estado == EstadoRespuestaApi.ApiNoDisponible)
        {
            // RW-06: el listado se muestra vacío, con un aviso en la misma pantalla.
            ViewData["ApiNoDisponible"] = true;
            return View(new List<LibroDto>());
        }

        return View(respuesta.Valor ?? new List<LibroDto>());
    }

    // GET /Libros/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        RespuestaApi<LibroDto> respuesta = await _librosApi.ObtenerAsync(id);
        switch (respuesta.Estado)
        {
            case EstadoRespuestaApi.Ok:
                return View(respuesta.Valor);

            case EstadoRespuestaApi.NoEncontrado:
                // RW-05: se vuelve al listado con un mensaje.
                TempData[ClaveMensajeError] = MensajeLibroNoEncontrado;
                return RedirectToAction(nameof(Index));

            case EstadoRespuestaApi.ApiNoDisponible:
                return View("ApiNoDisponible");

            default:
                throw new InvalidOperationException($"Estado no esperado: {respuesta.Estado}");
        }
    }

    // GET /Libros/Crear
    public IActionResult Crear()
    {
        // Se propone 1 ejemplar para que el formulario no empiece con un valor inválido (0).
        return View(new CrearLibroDto { CantidadEjemplares = 1 });
    }

    // POST /Libros/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearLibroDto dto)
    {
        // Errores simples (campo vacío, ejemplares fuera de rango): no hace falta llamar a la Api.
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        RespuestaApi<LibroDto> respuesta = await _librosApi.CrearAsync(dto);
        switch (respuesta.Estado)
        {
            case EstadoRespuestaApi.Ok:
                TempData[ClaveMensajeExito] = $"Se creó el libro \"{respuesta.Valor!.Titulo}\".";
                return RedirectToAction(nameof(Index));

            case EstadoRespuestaApi.Invalido:
            case EstadoRespuestaApi.IsbnDuplicado:
                // RW-03 y RW-04: se vuelve al formulario con lo escrito y el error junto a cada campo.
                CopiarErroresAModelState(respuesta.Errores);
                return View(dto);

            case EstadoRespuestaApi.ApiNoDisponible:
                // RW-06: el mensaje sale arriba del formulario (validation summary).
                ModelState.AddModelError(string.Empty, MensajeApiNoDisponible);
                return View(dto);

            default:
                throw new InvalidOperationException($"Estado no esperado: {respuesta.Estado}");
        }
    }

    // GET /Libros/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        RespuestaApi<LibroDto> respuesta = await _librosApi.ObtenerAsync(id);
        switch (respuesta.Estado)
        {
            case EstadoRespuestaApi.Ok:
                LibroDto libro = respuesta.Valor!;
                var dto = new ActualizarLibroDto
                {
                    Titulo = libro.Titulo,
                    Autor = libro.Autor,
                    Isbn = libro.Isbn,
                    AnioPublicacion = libro.AnioPublicacion,
                    CantidadEjemplares = libro.CantidadEjemplares
                };
                return View(dto);

            case EstadoRespuestaApi.NoEncontrado:
                TempData[ClaveMensajeError] = MensajeLibroNoEncontrado;
                return RedirectToAction(nameof(Index));

            case EstadoRespuestaApi.ApiNoDisponible:
                return View("ApiNoDisponible");

            default:
                throw new InvalidOperationException($"Estado no esperado: {respuesta.Estado}");
        }
    }

    // POST /Libros/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ActualizarLibroDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        RespuestaApi<LibroDto> respuesta = await _librosApi.ActualizarAsync(id, dto);
        switch (respuesta.Estado)
        {
            case EstadoRespuestaApi.Ok:
                TempData[ClaveMensajeExito] = $"Se guardaron los cambios de \"{respuesta.Valor!.Titulo}\".";
                return RedirectToAction(nameof(Index));

            case EstadoRespuestaApi.Invalido:
            case EstadoRespuestaApi.IsbnDuplicado:
                CopiarErroresAModelState(respuesta.Errores);
                return View(dto);

            case EstadoRespuestaApi.NoEncontrado:
                // RW-05: otro usuario lo borró mientras se editaba.
                TempData[ClaveMensajeError] = MensajeLibroNoEncontrado;
                return RedirectToAction(nameof(Index));

            case EstadoRespuestaApi.ApiNoDisponible:
                ModelState.AddModelError(string.Empty, MensajeApiNoDisponible);
                return View(dto);

            default:
                throw new InvalidOperationException($"Estado no esperado: {respuesta.Estado}");
        }
    }

    // POST /Libros/Eliminar/5
    // No hay página de confirmación: la pide site.js con SweetAlert2 antes de enviar el
    // formulario (RW-07). Si el usuario cancela, esta acción ni se llama.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        RespuestaApi<LibroDto> respuesta = await _librosApi.EliminarAsync(id);
        switch (respuesta.Estado)
        {
            case EstadoRespuestaApi.Ok:
                TempData[ClaveMensajeExito] = "El libro se eliminó correctamente.";
                break;

            case EstadoRespuestaApi.NoEncontrado:
                TempData[ClaveMensajeError] = MensajeLibroNoEncontrado;
                break;

            case EstadoRespuestaApi.ApiNoDisponible:
                TempData[ClaveMensajeError] =
                    "No se pudo conectar con el servidor de la biblioteca (Api). El libro no se eliminó.";
                break;

            default:
                throw new InvalidOperationException($"Estado no esperado: {respuesta.Estado}");
        }

        // En todos los casos se vuelve al listado; el mensaje dice qué pasó.
        return RedirectToAction(nameof(Index));
    }

    // Las claves de los errores de la Api ("Titulo", "Isbn"...) coinciden con los nombres de los
    // campos del formulario, así cada mensaje aparece junto a su campo.
    private void CopiarErroresAModelState(Dictionary<string, string[]> errores)
    {
        foreach (KeyValuePair<string, string[]> error in errores)
        {
            foreach (string mensaje in error.Value)
            {
                ModelState.AddModelError(error.Key, mensaje);
            }
        }
    }
}
