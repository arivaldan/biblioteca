using Biblioteca.Api.Entidades;
using Biblioteca.Api.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Tests.Datos;

public class BibliotecaDbContextTests
{
    [Fact]
    public async Task GuardarLibro_SePuedeLeerDespues()
    {
        using var bd = new BaseDeDatosEnMemoria();

        using (var contexto = bd.CrearContexto())
        {
            contexto.Libros.Add(CrearLibro("9788437604947"));
            await contexto.SaveChangesAsync();
        }

        using (var contexto = bd.CrearContexto())
        {
            Libro libro = await contexto.Libros.SingleAsync();
            Assert.True(libro.Id > 0);
            Assert.Equal("Cien años de soledad", libro.Titulo);
            Assert.Equal("Gabriel García Márquez", libro.Autor);
            Assert.Equal("9788437604947", libro.Isbn);
            Assert.Equal(1967, libro.AnioPublicacion);
            Assert.Equal(3, libro.CantidadEjemplares);
        }
    }

    // Red de seguridad de RN-04: aunque el servicio no revisara, la base no acepta duplicados.
    [Fact]
    public async Task GuardarDosLibrosConMismoIsbn_LaBaseLoRechaza()
    {
        using var bd = new BaseDeDatosEnMemoria();
        using var contexto = bd.CrearContexto();

        contexto.Libros.Add(CrearLibro("9788437604947"));
        contexto.Libros.Add(CrearLibro("9788437604947"));

        await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync());
    }

    private static Libro CrearLibro(string isbn)
    {
        return new Libro
        {
            Titulo = "Cien años de soledad",
            Autor = "Gabriel García Márquez",
            Isbn = isbn,
            AnioPublicacion = 1967,
            CantidadEjemplares = 3
        };
    }
}
