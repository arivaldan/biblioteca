using System.Net;
using System.Net.Http.Json;
using Biblioteca.Api.Tests.Infraestructura;
using Biblioteca.Contracts.Libros;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Tests.Libros;

/// <summary>
/// Tests HTTP: comprueban el "contrato" con el cliente (códigos de estado, Location,
/// ProblemDetails y DataAnnotations). El detalle de cada regla se prueba en LibroServiceTests.
/// Cada test crea su propia Api con una base vacía.
/// </summary>
public class LibrosControllerTests
{
    private const string Ruta = "/api/libros";

    // --- GET ------------------------------------------------------------------

    [Fact]
    public async Task Get_Listar_Devuelve200ConLosLibros()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        await CrearLibroAsync(cliente, CrearDto("9788437604947"));

        HttpResponseMessage respuesta = await cliente.GetAsync(Ruta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        List<LibroDto>? libros = await respuesta.Content.ReadFromJsonAsync<List<LibroDto>>();
        Assert.NotNull(libros);
        Assert.Single(libros);
    }

    [Fact]
    public async Task Get_IdExistente_Devuelve200ConElLibro()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        LibroDto creado = await CrearLibroAsync(cliente, CrearDto("9788437604947"));

        HttpResponseMessage respuesta = await cliente.GetAsync($"{Ruta}/{creado.Id}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        LibroDto? libro = await respuesta.Content.ReadFromJsonAsync<LibroDto>();
        Assert.Equal("Rayuela", libro?.Titulo);
    }

    [Fact]
    public async Task Get_IdInexistente_Devuelve404()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();

        HttpResponseMessage respuesta = await cliente.GetAsync($"{Ruta}/999");

        await AssertProblemDetailsAsync(respuesta, HttpStatusCode.NotFound);
    }

    // --- POST -----------------------------------------------------------------

    [Fact]
    public async Task Post_LibroValido_Devuelve201ConLocation()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync(Ruta, CrearDto("978-84-376-0494-7"));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        LibroDto? libro = await respuesta.Content.ReadFromJsonAsync<LibroDto>();
        Assert.NotNull(libro);
        Assert.Equal("9788437604947", libro.Isbn);
        Assert.NotNull(respuesta.Headers.Location);
        Assert.Equal($"{Ruta}/{libro.Id}", respuesta.Headers.Location.AbsolutePath);
    }

    // RN-01 (lo rechaza [Required] del DTO)
    [Fact]
    public async Task Post_TituloSoloEspacios_Devuelve400()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        CrearLibroDto dto = CrearDto("9788437604947");
        dto.Titulo = "   ";

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync(Ruta, dto);

        await AssertErrorDeValidacionAsync(respuesta, "Titulo");
    }

    // RN-02 (lo rechaza [Required] del DTO)
    [Fact]
    public async Task Post_AutorSoloEspacios_Devuelve400()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        CrearLibroDto dto = CrearDto("9788437604947");
        dto.Autor = "   ";

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync(Ruta, dto);

        await AssertErrorDeValidacionAsync(respuesta, "Autor");
    }

    // RN-03 (lo rechaza el servicio; debe salir con la misma forma que los DataAnnotations)
    [Fact]
    public async Task Post_IsbnInvalido_Devuelve400ConCampoIsbn()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync(Ruta, CrearDto("12345"));

        await AssertErrorDeValidacionAsync(respuesta, "Isbn");
    }

    // RN-04
    [Fact]
    public async Task Post_IsbnRepetido_Devuelve409ProblemDetails()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        await CrearLibroAsync(cliente, CrearDto("9788437604947"));

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync(Ruta, CrearDto("9788437604947"));

        await AssertProblemDetailsAsync(respuesta, HttpStatusCode.Conflict);
    }

    // RN-06 (lo rechaza [Range] del DTO)
    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Post_CantidadFueraDeRango_Devuelve400(int cantidad)
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        CrearLibroDto dto = CrearDto("9788437604947");
        dto.CantidadEjemplares = cantidad;

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync(Ruta, dto);

        await AssertErrorDeValidacionAsync(respuesta, "CantidadEjemplares");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Post_CantidadEnRango_Devuelve201(int cantidad)
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        CrearLibroDto dto = CrearDto("9788437604947");
        dto.CantidadEjemplares = cantidad;

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync(Ruta, dto);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    // --- PUT ------------------------------------------------------------------

    [Fact]
    public async Task Put_LibroValido_Devuelve200ConLibro()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        LibroDto creado = await CrearLibroAsync(cliente, CrearDto("9788437604947"));
        ActualizarLibroDto cambios = CrearDtoActualizar("9788437604947");
        cambios.Titulo = "Rayuela (edición revisada)";

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync($"{Ruta}/{creado.Id}", cambios);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        LibroDto? libro = await respuesta.Content.ReadFromJsonAsync<LibroDto>();
        Assert.Equal(creado.Id, libro?.Id);
        Assert.Equal("Rayuela (edición revisada)", libro?.Titulo);
    }

    [Fact]
    public async Task Put_IdInexistente_Devuelve404()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync($"{Ruta}/999", CrearDtoActualizar("9788437604947"));

        await AssertProblemDetailsAsync(respuesta, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_IsbnDeOtroLibro_Devuelve409()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        LibroDto aActualizar = await CrearLibroAsync(cliente, CrearDto("9788437604947"));
        await CrearLibroAsync(cliente, CrearDto("080442957X"));

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync($"{Ruta}/{aActualizar.Id}", CrearDtoActualizar("080442957X"));

        await AssertProblemDetailsAsync(respuesta, HttpStatusCode.Conflict);
    }

    // --- DELETE ---------------------------------------------------------------

    [Fact]
    public async Task Delete_LibroExistente_Devuelve204YLuegoNoExiste()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();
        LibroDto creado = await CrearLibroAsync(cliente, CrearDto("9788437604947"));

        HttpResponseMessage respuesta = await cliente.DeleteAsync($"{Ruta}/{creado.Id}");

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        HttpResponseMessage despues = await cliente.GetAsync($"{Ruta}/{creado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, despues.StatusCode);
    }

    [Fact]
    public async Task Delete_IdInexistente_Devuelve404()
    {
        using var api = new BibliotecaApiFactory();
        HttpClient cliente = api.CreateClient();

        HttpResponseMessage respuesta = await cliente.DeleteAsync($"{Ruta}/999");

        await AssertProblemDetailsAsync(respuesta, HttpStatusCode.NotFound);
    }

    // --- Ayudas ---------------------------------------------------------------

    private static CrearLibroDto CrearDto(string isbn)
    {
        return new CrearLibroDto
        {
            Titulo = "Rayuela",
            Autor = "Julio Cortázar",
            Isbn = isbn,
            AnioPublicacion = 1963,
            CantidadEjemplares = 2
        };
    }

    private static ActualizarLibroDto CrearDtoActualizar(string isbn)
    {
        return new ActualizarLibroDto
        {
            Titulo = "Rayuela",
            Autor = "Julio Cortázar",
            Isbn = isbn,
            AnioPublicacion = 1963,
            CantidadEjemplares = 2
        };
    }

    // Crea un libro por la API y falla el test si no se pudo (para preparar datos).
    private static async Task<LibroDto> CrearLibroAsync(HttpClient cliente, CrearLibroDto dto)
    {
        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync(Ruta, dto);
        respuesta.EnsureSuccessStatusCode();
        LibroDto? libro = await respuesta.Content.ReadFromJsonAsync<LibroDto>();
        Assert.NotNull(libro);
        return libro;
    }

    private static async Task AssertProblemDetailsAsync(HttpResponseMessage respuesta, HttpStatusCode codigoEsperado)
    {
        Assert.Equal(codigoEsperado, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        ProblemDetails? problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal((int)codigoEsperado, problema?.Status);
    }

    private static async Task AssertErrorDeValidacionAsync(HttpResponseMessage respuesta, string campo)
    {
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        ValidationProblemDetails? problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problema);
        Assert.True(problema.Errors.ContainsKey(campo),
            $"Se esperaba un error en '{campo}'. Campos con error: {string.Join(", ", problema.Errors.Keys)}");
    }
}
