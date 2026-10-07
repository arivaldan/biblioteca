using Biblioteca.Api.Libros;
using Biblioteca.Contracts.Libros;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers;

/// <summary>
/// CRUD de libros. No tiene lógica de negocio: llama a LibroService y traduce lo que
/// devuelve a códigos HTTP. Todos los errores salen en formato ProblemDetails.
/// </summary>
/// <remarks>
/// [ApiController] valida los DataAnnotations del DTO antes de entrar a cada acción y,
/// si fallan, responde 400 automáticamente.
/// </remarks>
[ApiController]
[Route("api/libros")]
public class LibrosController : ControllerBase
{
    private readonly LibroService _libroService;

    public LibrosController(LibroService libroService)
    {
        _libroService = libroService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LibroDto>>> Listar()
    {
        List<LibroDto> libros = await _libroService.ListarAsync();
        return Ok(libros);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LibroDto>> Obtener(int id)
    {
        LibroDto? libro = await _libroService.ObtenerPorIdAsync(id);
        if (libro == null)
        {
            return RespuestaNoEncontrado(id);
        }

        return Ok(libro);
    }

    [HttpPost]
    public async Task<ActionResult<LibroDto>> Crear(CrearLibroDto dto)
    {
        Resultado<LibroDto> resultado = await _libroService.CrearAsync(dto);
        if (resultado.Estado != EstadoResultado.Ok)
        {
            return RespuestaDeError(resultado, id: null);
        }

        // Con Estado Ok, Valor siempre tiene dato.
        LibroDto libro = resultado.Valor!;

        // 201 Created + cabecera Location apuntando a GET /api/libros/{id}.
        return CreatedAtAction(nameof(Obtener), new { id = libro.Id }, libro);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LibroDto>> Actualizar(int id, ActualizarLibroDto dto)
    {
        Resultado<LibroDto> resultado = await _libroService.ActualizarAsync(id, dto);
        if (resultado.Estado != EstadoResultado.Ok)
        {
            return RespuestaDeError(resultado, id);
        }

        return Ok(resultado.Valor);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        Resultado<LibroDto> resultado = await _libroService.EliminarAsync(id);
        if (resultado.Estado != EstadoResultado.Ok)
        {
            return RespuestaDeError(resultado, id);
        }

        return NoContent();
    }

    // Único lugar donde se decide qué código HTTP corresponde a cada error del servicio.
    private ActionResult RespuestaDeError(Resultado<LibroDto> resultado, int? id)
    {
        switch (resultado.Estado)
        {
            case EstadoResultado.Invalido:
                // Se pasan los errores a ModelState para que el 400 tenga exactamente la misma
                // forma que el 400 automático de los DataAnnotations.
                foreach (KeyValuePair<string, string[]> error in resultado.Errores)
                {
                    foreach (string mensaje in error.Value)
                    {
                        ModelState.AddModelError(error.Key, mensaje);
                    }
                }
                return ValidationProblem(ModelState);

            case EstadoResultado.NoEncontrado:
                return RespuestaNoEncontrado(id);

            case EstadoResultado.IsbnDuplicado:
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "ISBN duplicado",
                    detail: "Ya hay otro libro registrado con ese ISBN.");

            default:
                throw new InvalidOperationException($"Estado no esperado: {resultado.Estado}");
        }
    }

    private ObjectResult RespuestaNoEncontrado(int? id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Libro no encontrado",
            detail: $"No existe un libro con id {id}.");
    }
}
