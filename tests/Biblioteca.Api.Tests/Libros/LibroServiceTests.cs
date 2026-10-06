using Biblioteca.Api.Entidades;
using Biblioteca.Api.Libros;
using Biblioteca.Api.Tests.Infraestructura;
using Biblioteca.Contracts.Libros;

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

    // --- Ayudas ---------------------------------------------------------------

    // El servicio usa su propio contexto, distinto del que guardó los datos, para que lea
    // de verdad de la base y no de la memoria de EF.
    private static LibroService CrearServicio(BaseDeDatosEnMemoria bd)
    {
        return new LibroService(bd.CrearContexto());
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
