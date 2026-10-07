using System.Net;
using Biblioteca.Contracts.Libros;
using Biblioteca.Web.ApiClients;
using Biblioteca.Web.Tests.Infraestructura;

namespace Biblioteca.Web.Tests.ApiClients;

public class LibrosApiClientTests
{
    // JSON con la misma forma que devuelve la Api (camelCase).
    private const string LibroJson =
        """{"id":1,"titulo":"Rayuela","autor":"Julio Cortázar","isbn":"9788437604947","anioPublicacion":1963,"cantidadEjemplares":2}""";

    // --- Respuestas correctas -------------------------------------------------

    [Fact]
    public async Task Listar_200_DevuelveLosLibros()
    {
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.OK, $"[{LibroJson}]");
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<List<LibroDto>> respuesta = await cliente.ListarAsync();

        Assert.Equal(EstadoRespuestaApi.Ok, respuesta.Estado);
        LibroDto libro = Assert.Single(respuesta.Valor!);
        Assert.Equal("Rayuela", libro.Titulo);
        Assert.Equal(HttpMethod.Get, handler.MetodoRecibido);
        Assert.Equal("/api/libros", handler.RutaRecibida);
    }

    [Fact]
    public async Task Obtener_200_DevuelveElLibro()
    {
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.OK, LibroJson);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.ObtenerAsync(1);

        Assert.Equal(EstadoRespuestaApi.Ok, respuesta.Estado);
        Assert.Equal(1, respuesta.Valor!.Id);
        Assert.Equal("Julio Cortázar", respuesta.Valor.Autor);
        Assert.Equal(1963, respuesta.Valor.AnioPublicacion);
        Assert.Equal(HttpMethod.Get, handler.MetodoRecibido);
        Assert.Equal("/api/libros/1", handler.RutaRecibida);
    }

    [Fact]
    public async Task Crear_201_DevuelveElLibroCreadoYEnviaElDto()
    {
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.Created, LibroJson);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.CrearAsync(CrearDtoValido());

        Assert.Equal(EstadoRespuestaApi.Ok, respuesta.Estado);
        Assert.Equal(1, respuesta.Valor!.Id);
        Assert.Equal(HttpMethod.Post, handler.MetodoRecibido);
        Assert.Equal("/api/libros", handler.RutaRecibida);
        Assert.Contains("\"titulo\":\"Rayuela\"", handler.CuerpoRecibido);
    }

    [Fact]
    public async Task Actualizar_200_DevuelveElLibroActualizado()
    {
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.OK, LibroJson);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.ActualizarAsync(1, ActualizarDtoValido());

        Assert.Equal(EstadoRespuestaApi.Ok, respuesta.Estado);
        Assert.Equal("Rayuela", respuesta.Valor!.Titulo);
        Assert.Equal(HttpMethod.Put, handler.MetodoRecibido);
        Assert.Equal("/api/libros/1", handler.RutaRecibida);
        Assert.Contains("\"isbn\":\"9788437604947\"", handler.CuerpoRecibido);
    }

    [Fact]
    public async Task Eliminar_204_DevuelveOkSinValor()
    {
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.NoContent);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.EliminarAsync(1);

        Assert.Equal(EstadoRespuestaApi.Ok, respuesta.Estado);
        Assert.Null(respuesta.Valor);
        Assert.Equal(HttpMethod.Delete, handler.MetodoRecibido);
        Assert.Equal("/api/libros/1", handler.RutaRecibida);
    }

    // --- RW-03: 400 con errores por campo -------------------------------------

    [Fact]
    public async Task Crear_400_DevuelveLosErroresPorCampo()
    {
        const string problemaJson = """
            {
              "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
              "title": "One or more validation errors occurred.",
              "status": 400,
              "errors": {
                "Isbn": ["El ISBN debe tener 13 dígitos, o 10 caracteres donde solo el último puede ser X."],
                "Titulo": ["El título es obligatorio."]
              }
            }
            """;
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.BadRequest, problemaJson);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.CrearAsync(CrearDtoValido());

        Assert.Equal(EstadoRespuestaApi.Invalido, respuesta.Estado);
        Assert.Equal(["El título es obligatorio."], respuesta.Errores["Titulo"]);
        Assert.Single(respuesta.Errores["Isbn"]);
    }

    // --- RW-04: 409 como error del campo Isbn ---------------------------------

    [Fact]
    public async Task Crear_409_DevuelveIsbnDuplicadoConErrorEnIsbn()
    {
        const string problemaJson = """
            {"title":"ISBN duplicado","status":409,"detail":"Ya hay otro libro registrado con ese ISBN."}
            """;
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.Conflict, problemaJson);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.CrearAsync(CrearDtoValido());

        Assert.Equal(EstadoRespuestaApi.IsbnDuplicado, respuesta.Estado);
        Assert.Equal(["Ya hay otro libro registrado con ese ISBN."], respuesta.Errores["Isbn"]);
    }

    [Fact]
    public async Task Actualizar_409_DevuelveIsbnDuplicado()
    {
        const string problemaJson = """{"title":"ISBN duplicado","status":409,"detail":"Ya hay otro libro registrado con ese ISBN."}""";
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.Conflict, problemaJson);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.ActualizarAsync(1, ActualizarDtoValido());

        Assert.Equal(EstadoRespuestaApi.IsbnDuplicado, respuesta.Estado);
        Assert.True(respuesta.Errores.ContainsKey("Isbn"));
    }

    // --- RW-05: 404 -----------------------------------------------------------

    [Fact]
    public async Task Obtener_404_DevuelveNoEncontrado()
    {
        const string problemaJson = """{"title":"Libro no encontrado","status":404,"detail":"No existe un libro con id 99."}""";
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.NotFound, problemaJson);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.ObtenerAsync(99);

        Assert.Equal(EstadoRespuestaApi.NoEncontrado, respuesta.Estado);
        Assert.Null(respuesta.Valor);
    }

    [Fact]
    public async Task Eliminar_404_DevuelveNoEncontrado()
    {
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.NotFound);
        LibrosApiClient cliente = CrearCliente(handler);

        RespuestaApi<LibroDto> respuesta = await cliente.EliminarAsync(99);

        Assert.Equal(EstadoRespuestaApi.NoEncontrado, respuesta.Estado);
    }

    // --- RW-06: Api no disponible ---------------------------------------------

    [Fact]
    public async Task Listar_ApiApagada_DevuelveApiNoDisponible()
    {
        LibrosApiClient cliente = CrearCliente(HttpMessageHandlerFalso.ConApiApagada());

        RespuestaApi<List<LibroDto>> respuesta = await cliente.ListarAsync();

        Assert.Equal(EstadoRespuestaApi.ApiNoDisponible, respuesta.Estado);
    }

    [Fact]
    public async Task Crear_ApiApagada_DevuelveApiNoDisponible()
    {
        LibrosApiClient cliente = CrearCliente(HttpMessageHandlerFalso.ConApiApagada());

        RespuestaApi<LibroDto> respuesta = await cliente.CrearAsync(CrearDtoValido());

        Assert.Equal(EstadoRespuestaApi.ApiNoDisponible, respuesta.Estado);
    }

    // --- Códigos no esperados -------------------------------------------------

    [Fact]
    public async Task Listar_500_LanzaExcepcion()
    {
        // Un 500 es un fallo de la Api que el usuario no puede corregir: se deja subir la
        // excepción para que se vea la página de error.
        var handler = HttpMessageHandlerFalso.QueResponde(HttpStatusCode.InternalServerError);
        LibrosApiClient cliente = CrearCliente(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => cliente.ListarAsync());
    }

    // --- Ayudas ---------------------------------------------------------------

    private static LibrosApiClient CrearCliente(HttpMessageHandlerFalso handler)
    {
        // La dirección no importa: el handler falso nunca sale a la red.
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://api.falsa/")
        };
        return new LibrosApiClient(httpClient);
    }

    private static CrearLibroDto CrearDtoValido()
    {
        return new CrearLibroDto
        {
            Titulo = "Rayuela",
            Autor = "Julio Cortázar",
            Isbn = "9788437604947",
            AnioPublicacion = 1963,
            CantidadEjemplares = 2
        };
    }

    private static ActualizarLibroDto ActualizarDtoValido()
    {
        return new ActualizarLibroDto
        {
            Titulo = "Rayuela",
            Autor = "Julio Cortázar",
            Isbn = "9788437604947",
            AnioPublicacion = 1963,
            CantidadEjemplares = 2
        };
    }
}
