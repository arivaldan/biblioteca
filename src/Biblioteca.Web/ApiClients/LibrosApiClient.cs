using System.Net;
using System.Net.Http.Json;
using Biblioteca.Contracts.Libros;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.ApiClients;

/// <summary>
/// HttpClient tipado para /api/libros. Es el único lugar de la Web que sabe hablar con la Api
/// de libros (RW-01): los controllers solo ven RespuestaApi.
/// </summary>
/// <remarks>
/// La dirección de la Api (BaseAddress) se configura en Program.cs a partir de appsettings
/// (RW-02), por eso aquí solo se usan rutas relativas.
/// </remarks>
public class LibrosApiClient
{
    private const string RutaLibros = "api/libros";

    private readonly HttpClient _httpClient;

    public LibrosApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RespuestaApi<List<LibroDto>>> ListarAsync()
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Get, RutaLibros);
        return await EnviarAsync<List<LibroDto>>(solicitud);
    }

    public async Task<RespuestaApi<LibroDto>> ObtenerAsync(int id)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Get, $"{RutaLibros}/{id}");
        return await EnviarAsync<LibroDto>(solicitud);
    }

    public async Task<RespuestaApi<LibroDto>> CrearAsync(CrearLibroDto dto)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Post, RutaLibros)
        {
            Content = JsonContent.Create(dto)
        };
        return await EnviarAsync<LibroDto>(solicitud);
    }

    public async Task<RespuestaApi<LibroDto>> ActualizarAsync(int id, ActualizarLibroDto dto)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Put, $"{RutaLibros}/{id}")
        {
            Content = JsonContent.Create(dto)
        };
        return await EnviarAsync<LibroDto>(solicitud);
    }

    /// <summary>
    /// Elimina un libro. Si sale bien, Valor es null: la Api responde 204 sin cuerpo.
    /// </summary>
    public async Task<RespuestaApi<LibroDto>> EliminarAsync(int id)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Delete, $"{RutaLibros}/{id}");
        return await EnviarAsync<LibroDto>(solicitud);
    }

    // Envía la solicitud y traduce la respuesta HTTP a RespuestaApi. Es el único lugar donde
    // se decide qué significa cada código de la Api.
    private async Task<RespuestaApi<T>> EnviarAsync<T>(HttpRequestMessage solicitud)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await _httpClient.SendAsync(solicitud);
        }
        catch (HttpRequestException)
        {
            // La Api no respondió: está apagada o la dirección en appsettings es incorrecta (RW-06).
            return RespuestaApi<T>.ApiNoDisponible();
        }

        switch (respuesta.StatusCode)
        {
            case HttpStatusCode.OK:
            case HttpStatusCode.Created:
                T? valor = await respuesta.Content.ReadFromJsonAsync<T>();
                return RespuestaApi<T>.Ok(valor);

            case HttpStatusCode.NoContent:
                return RespuestaApi<T>.Ok(default);

            case HttpStatusCode.NotFound:
                return RespuestaApi<T>.NoEncontrado();

            case HttpStatusCode.BadRequest:
                // La Api devuelve ValidationProblemDetails: "errors" trae los mensajes por campo.
                ValidationProblemDetails? problemaValidacion =
                    await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                var errores = new Dictionary<string, string[]>(
                    problemaValidacion?.Errors ?? new Dictionary<string, string[]>());
                return RespuestaApi<T>.Invalido(errores);

            case HttpStatusCode.Conflict:
                // El 409 solo ocurre por ISBN duplicado (RN-04). Se devuelve como error del
                // campo Isbn para que el formulario lo muestre junto a ese campo (RW-04).
                ProblemDetails? problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
                string mensaje = problema?.Detail ?? "Ya hay otro libro registrado con ese ISBN.";
                var erroresIsbn = new Dictionary<string, string[]>
                {
                    ["Isbn"] = [mensaje]
                };
                return RespuestaApi<T>.IsbnDuplicado(erroresIsbn);

            default:
                // Un código no esperado (por ejemplo, 500) es un fallo de la Api, no algo que el
                // usuario pueda corregir: se lanza la excepción y se ve la página de error.
                respuesta.EnsureSuccessStatusCode();
                throw new InvalidOperationException($"Código de respuesta no esperado: {(int)respuesta.StatusCode}");
        }
    }
}
