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
}
