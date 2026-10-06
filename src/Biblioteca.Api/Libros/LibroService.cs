using Biblioteca.Api.Datos;
using Biblioteca.Api.Entidades;
using Biblioteca.Contracts.Libros;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Libros;

/// <summary>
/// Lógica de negocio de los libros (reglas RN-01 a RN-09 de docs/specs/libro.md).
/// El controller solo traduce lo que devuelve este servicio a respuestas HTTP.
/// </summary>
public class LibroService
{
    private readonly BibliotecaDbContext _contexto;

    public LibroService(BibliotecaDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <summary>
    /// Todos los libros, ordenados por título ascendente.
    /// </summary>
    public async Task<List<LibroDto>> ListarAsync()
    {
        // AsNoTracking: solo vamos a leer, así que EF no necesita guardar una copia de cada
        // libro para detectar cambios. Ahorra memoria y deja clara la intención.
        List<Libro> libros = await _contexto.Libros
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToListAsync();

        return libros.Select(ADto).ToList();
    }

    /// <summary>
    /// El libro con ese id, o null si no existe (el controller lo convierte en 404).
    /// </summary>
    public async Task<LibroDto?> ObtenerPorIdAsync(int id)
    {
        Libro? libro = await _contexto.Libros
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (libro == null)
        {
            return null;
        }

        return ADto(libro);
    }

    private static LibroDto ADto(Libro libro)
    {
        return new LibroDto
        {
            Id = libro.Id,
            Titulo = libro.Titulo,
            Autor = libro.Autor,
            Isbn = libro.Isbn,
            AnioPublicacion = libro.AnioPublicacion,
            CantidadEjemplares = libro.CantidadEjemplares
        };
    }
}
