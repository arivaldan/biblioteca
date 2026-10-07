using Biblioteca.Api.Entidades;
using Biblioteca.Api.Libros;
using Biblioteca.Api.Tests.Infraestructura;
using Biblioteca.Contracts.Libros;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Tests.Libros;

public class LibroServiceTests
{
    // --- Listar ---------------------------------------------------------------

    [Fact]
    public async Task Listar_DevuelveTodosOrdenadosPorTitulo()
    {
        using var bd = new BaseDeDatosEnMemoria();
        // Se insertan desordenados a propósito. Todos los títulos empiezan con mayúscula
        // porque SQLite distingue mayúsculas al ordenar y SQL Server no.
        await GuardarLibrosAsync(bd,
            CrearLibro("Rayuela", "9788437604947"),
            CrearLibro("El túnel", "8437604947"),
            CrearLibro("Ficciones", "080442957X"));
        LibroService servicio = CrearServicio(bd);

        List<LibroDto> libros = await servicio.ListarAsync();

        Assert.Equal(["El túnel", "Ficciones", "Rayuela"], libros.Select(l => l.Titulo));
    }

    [Fact]
    public async Task Listar_SinLibros_DevuelveListaVacia()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);

        List<LibroDto> libros = await servicio.ListarAsync();

        Assert.Empty(libros);
    }

    // --- Obtener por id -------------------------------------------------------

    [Fact]
    public async Task ObtenerPorId_Existente_DevuelveLibro()
    {
        using var bd = new BaseDeDatosEnMemoria();
        Libro guardado = CrearLibro("Rayuela", "9788437604947");
        await GuardarLibrosAsync(bd, guardado);
        LibroService servicio = CrearServicio(bd);

        LibroDto? libro = await servicio.ObtenerPorIdAsync(guardado.Id);

        Assert.NotNull(libro);
        Assert.Equal(guardado.Id, libro.Id);
        Assert.Equal("Rayuela", libro.Titulo);
        Assert.Equal("Julio Cortázar", libro.Autor);
        Assert.Equal("9788437604947", libro.Isbn);
        Assert.Equal(1963, libro.AnioPublicacion);
        Assert.Equal(2, libro.CantidadEjemplares);
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_DevuelveNull()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);

        LibroDto? libro = await servicio.ObtenerPorIdAsync(999);

        Assert.Null(libro);
    }

    // --- Crear: caso feliz ----------------------------------------------------

    [Fact]
    public async Task Crear_LibroValido_SeGuardaYDevuelveElLibroConId()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);

        Resultado<LibroDto> resultado = await servicio.CrearAsync(CrearDto());

        Assert.Equal(EstadoResultado.Ok, resultado.Estado);
        Assert.NotNull(resultado.Valor);
        Assert.True(resultado.Valor.Id > 0);
        Libro guardado = await LeerUnicoLibroAsync(bd);
        Assert.Equal("Rayuela", guardado.Titulo);
        Assert.Equal("Julio Cortázar", guardado.Autor);
        Assert.Equal("9788437604947", guardado.Isbn);
        Assert.Equal(1963, guardado.AnioPublicacion);
        Assert.Equal(2, guardado.CantidadEjemplares);
    }

    // --- Crear: RN-01 Titulo --------------------------------------------------

    [Fact]
    public async Task Crear_TituloConEspacios_SeGuardaRecortado()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Titulo = "   Rayuela  ";

        await servicio.CrearAsync(dto);

        Libro guardado = await LeerUnicoLibroAsync(bd);
        Assert.Equal("Rayuela", guardado.Titulo);
    }

    [Fact]
    public async Task Crear_TituloSoloEspacios_EsInvalido()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Titulo = "    ";

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        AssertInvalidoEnCampo(resultado, "Titulo");
    }

    [Fact]
    public async Task Crear_Titulo200Caracteres_EsValido()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        // Los espacios alrededor no cuentan: el largo se mide después del recorte.
        dto.Titulo = "  " + new string('a', 200) + "  ";

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        Assert.Equal(EstadoResultado.Ok, resultado.Estado);
    }

    [Fact]
    public async Task Crear_Titulo201Caracteres_EsInvalido()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Titulo = new string('a', 201);

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        AssertInvalidoEnCampo(resultado, "Titulo");
    }

    // --- Crear: RN-02 Autor ---------------------------------------------------

    [Fact]
    public async Task Crear_AutorConEspacios_SeGuardaRecortado()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Autor = "  Julio Cortázar ";

        await servicio.CrearAsync(dto);

        Libro guardado = await LeerUnicoLibroAsync(bd);
        Assert.Equal("Julio Cortázar", guardado.Autor);
    }

    [Fact]
    public async Task Crear_AutorSoloEspacios_EsInvalido()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Autor = "   ";

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        AssertInvalidoEnCampo(resultado, "Autor");
    }

    [Fact]
    public async Task Crear_Autor150Caracteres_EsValido()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Autor = new string('a', 150);

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        Assert.Equal(EstadoResultado.Ok, resultado.Estado);
    }

    [Fact]
    public async Task Crear_Autor151Caracteres_EsInvalido()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Autor = new string('a', 151);

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        AssertInvalidoEnCampo(resultado, "Autor");
    }

    // --- Crear: RN-03 y RN-07 ISBN --------------------------------------------

    [Fact]
    public async Task Crear_IsbnConFormatoInvalido_EsInvalido()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Isbn = "978843760494X";

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        AssertInvalidoEnCampo(resultado, "Isbn");
    }

    [Fact]
    public async Task Crear_IsbnConGuiones_SeGuardaNormalizado()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Isbn = "978-84-376 0494-7";

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        Assert.Equal("9788437604947", resultado.Valor?.Isbn);
        Libro guardado = await LeerUnicoLibroAsync(bd);
        Assert.Equal("9788437604947", guardado.Isbn);
    }

    [Fact]
    public async Task Crear_IsbnConXMinuscula_SeGuardaConXMayuscula()
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Isbn = "0-8044-2957-x";

        await servicio.CrearAsync(dto);

        Libro guardado = await LeerUnicoLibroAsync(bd);
        Assert.Equal("080442957X", guardado.Isbn);
    }

    // --- Crear: RN-04 ISBN único ----------------------------------------------

    [Fact]
    public async Task Crear_IsbnRepetido_DevuelveIsbnDuplicado()
    {
        using var bd = new BaseDeDatosEnMemoria();
        await GuardarLibrosAsync(bd, CrearLibro("Rayuela", "9788437604947"));
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Isbn = "9788437604947";

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        Assert.Equal(EstadoResultado.IsbnDuplicado, resultado.Estado);
    }

    [Fact]
    public async Task Crear_IsbnRepetidoConGuiones_DevuelveIsbnDuplicado()
    {
        using var bd = new BaseDeDatosEnMemoria();
        await GuardarLibrosAsync(bd, CrearLibro("Rayuela", "9788437604947"));
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.Isbn = "978-84-376-0494-7";

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        Assert.Equal(EstadoResultado.IsbnDuplicado, resultado.Estado);
    }

    // --- Crear: RN-05 Año de publicación --------------------------------------

    [Theory]
    [InlineData(1450)]       // mínimo permitido
    [InlineData(AnioActual)] // máximo permitido
    [InlineData(null)]       // es opcional
    public async Task Crear_AnioPublicacionPermitido_EsValido(int? anio)
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.AnioPublicacion = anio;

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        Assert.Equal(EstadoResultado.Ok, resultado.Estado);
    }

    [Theory]
    [InlineData(1449)]           // antes de la imprenta
    [InlineData(AnioActual + 1)] // año futuro
    public async Task Crear_AnioPublicacionFueraDeRango_EsInvalido(int anio)
    {
        using var bd = new BaseDeDatosEnMemoria();
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto();
        dto.AnioPublicacion = anio;

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        AssertInvalidoEnCampo(resultado, "AnioPublicacion");
    }

    // --- Crear: RN-08 ---------------------------------------------------------

    [Fact]
    public async Task Crear_MismoTituloYAutorDistintoIsbn_EsOk()
    {
        using var bd = new BaseDeDatosEnMemoria();
        await GuardarLibrosAsync(bd, CrearLibro("Rayuela", "8437604947"));
        LibroService servicio = CrearServicio(bd);
        CrearLibroDto dto = CrearDto(); // mismo título y autor, ISBN 9788437604947

        Resultado<LibroDto> resultado = await servicio.CrearAsync(dto);

        Assert.Equal(EstadoResultado.Ok, resultado.Estado);
    }

    // --- Ayudas ---------------------------------------------------------------

    // El reloj de los tests siempre marca una fecha de este año.
    private const int AnioActual = 2026;

    // El servicio usa su propio contexto, distinto del que guardó los datos, para que lea
    // de verdad de la base y no de la memoria de EF.
    private static LibroService CrearServicio(BaseDeDatosEnMemoria bd)
    {
        var reloj = new TimeProviderFijo(new DateTimeOffset(AnioActual, 6, 15, 12, 0, 0, TimeSpan.Zero));
        return new LibroService(bd.CrearContexto(), reloj);
    }

    // Un DTO válido; cada test cambia solo el campo que le interesa.
    private static CrearLibroDto CrearDto()
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

    private static void AssertInvalidoEnCampo(Resultado<LibroDto> resultado, string campo)
    {
        Assert.Equal(EstadoResultado.Invalido, resultado.Estado);
        Assert.True(resultado.Errores.ContainsKey(campo), $"Se esperaba un error en el campo '{campo}'.");
    }

    private static async Task<Libro> LeerUnicoLibroAsync(BaseDeDatosEnMemoria bd)
    {
        using var contexto = bd.CrearContexto();
        return await contexto.Libros.SingleAsync();
    }

    private static async Task GuardarLibrosAsync(BaseDeDatosEnMemoria bd, params Libro[] libros)
    {
        using var contexto = bd.CrearContexto();
        contexto.Libros.AddRange(libros);
        await contexto.SaveChangesAsync();
    }

    private static Libro CrearLibro(string titulo, string isbn)
    {
        return new Libro
        {
            Titulo = titulo,
            Autor = "Julio Cortázar",
            Isbn = isbn,
            AnioPublicacion = 1963,
            CantidadEjemplares = 2
        };
    }
}
