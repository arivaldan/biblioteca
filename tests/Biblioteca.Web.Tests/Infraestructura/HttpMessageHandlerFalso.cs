using System.Net;
using System.Text;

namespace Biblioteca.Web.Tests.Infraestructura;

/// <summary>
/// Reemplaza la red en los tests: en vez de llamar a la Api de verdad, devuelve siempre la
/// respuesta que se le indica y guarda la solicitud que recibió para poder revisarla.
/// </summary>
public class HttpMessageHandlerFalso : HttpMessageHandler
{
    private readonly HttpStatusCode _codigo;
    private readonly string? _cuerpoJson;
    private readonly bool _simularApiApagada;

    public HttpMethod? MetodoRecibido { get; private set; }

    public string? RutaRecibida { get; private set; }

    public string? CuerpoRecibido { get; private set; }

    private HttpMessageHandlerFalso(HttpStatusCode codigo, string? cuerpoJson, bool simularApiApagada)
    {
        _codigo = codigo;
        _cuerpoJson = cuerpoJson;
        _simularApiApagada = simularApiApagada;
    }

    /// <summary>Responde con el código indicado y, opcionalmente, un cuerpo JSON.</summary>
    public static HttpMessageHandlerFalso QueResponde(HttpStatusCode codigo, string? cuerpoJson = null)
    {
        return new HttpMessageHandlerFalso(codigo, cuerpoJson, simularApiApagada: false);
    }

    /// <summary>Simula que la Api no responde: lanza la misma excepción que HttpClient.</summary>
    public static HttpMessageHandlerFalso ConApiApagada()
    {
        return new HttpMessageHandlerFalso(HttpStatusCode.OK, null, simularApiApagada: true);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        MetodoRecibido = request.Method;
        RutaRecibida = request.RequestUri?.AbsolutePath;
        // El cuerpo se lee aquí porque HttpClient libera la solicitud al terminar.
        if (request.Content != null)
        {
            CuerpoRecibido = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        if (_simularApiApagada)
        {
            throw new HttpRequestException("No se puede establecer una conexión con la Api.");
        }

        var respuesta = new HttpResponseMessage(_codigo);
        if (_cuerpoJson != null)
        {
            respuesta.Content = new StringContent(_cuerpoJson, Encoding.UTF8, "application/json");
        }
        return respuesta;
    }
}
