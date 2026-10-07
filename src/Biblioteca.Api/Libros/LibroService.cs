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
    private const int LargoMaximoTitulo = 200;
    private const int LargoMaximoAutor = 150;
    private const int AnioMinimoPublicacion = 1450;

    private readonly BibliotecaDbContext _contexto;

    // Se recibe el reloj en lugar de usar DateTime.Now para que los tests puedan fijar
    // el "año actual" (RN-05).
    private readonly TimeProvider _reloj;

    public LibroService(BibliotecaDbContext contexto, TimeProvider reloj)
    {
        _contexto = contexto;
        _reloj = reloj;
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

    /// <summary>
    /// Crea un libro aplicando las reglas de la spec. Devuelve Invalido (400) si falla una
    /// validación, IsbnDuplicado (409) si el ISBN ya existe, u Ok con el libro creado.
    /// </summary>
    public async Task<Resultado<LibroDto>> CrearAsync(CrearLibroDto dto)
    {
        // Primero se limpian los datos (RN-01, RN-02, RN-07) y después se valida lo limpio.
        string titulo = dto.Titulo.Trim();
        string autor = dto.Autor.Trim();
        string isbn = Isbn.Normalizar(dto.Isbn);

        Dictionary<string, string[]> errores = Validar(titulo, autor, isbn, dto.AnioPublicacion);
        if (errores.Count > 0)
        {
            return Resultado<LibroDto>.Invalido(errores);
        }

        // RN-04. Se compara con el ISBN normalizado, que es como están guardados todos.
        bool isbnYaExiste = await _contexto.Libros.AnyAsync(l => l.Isbn == isbn);
        if (isbnYaExiste)
        {
            return Resultado<LibroDto>.IsbnDuplicado();
        }

        var libro = new Libro
        {
            Titulo = titulo,
            Autor = autor,
            Isbn = isbn,
            AnioPublicacion = dto.AnioPublicacion,
            CantidadEjemplares = dto.CantidadEjemplares
        };
        _contexto.Libros.Add(libro);
        await _contexto.SaveChangesAsync();

        return Resultado<LibroDto>.Ok(ADto(libro));
    }

    /// <summary>
    /// Reglas que necesitan el valor ya limpio o el reloj. Lo "obligatorio" y RN-06 los
    /// revisan los DataAnnotations del DTO antes de llegar aquí.
    /// Devuelve los errores por campo; si está vacío, todo es válido.
    /// </summary>
    private Dictionary<string, string[]> Validar(string titulo, string autor, string isbn, int? anioPublicacion)
    {
        var errores = new Dictionary<string, string[]>();

        // RN-01. Se repite el "obligatorio" porque el servicio también se puede usar sin
        // pasar por el controller (por ejemplo, en los tests).
        if (titulo.Length == 0)
        {
            errores["Titulo"] = ["El título es obligatorio."];
        }
        else if (titulo.Length > LargoMaximoTitulo)
        {
            errores["Titulo"] = [$"El título no puede tener más de {LargoMaximoTitulo} caracteres."];
        }

        // RN-02
        if (autor.Length == 0)
        {
            errores["Autor"] = ["El autor es obligatorio."];
        }
        else if (autor.Length > LargoMaximoAutor)
        {
            errores["Autor"] = [$"El autor no puede tener más de {LargoMaximoAutor} caracteres."];
        }

        // RN-03
        if (!Isbn.TieneFormatoValido(isbn))
        {
            errores["Isbn"] = ["El ISBN debe tener 13 dígitos, o 10 caracteres donde solo el último puede ser X."];
        }

        // RN-05. El año es opcional; solo se valida si viene. "Año actual" según el reloj
        // del servidor: no se aceptan años futuros.
        int anioActual = _reloj.GetLocalNow().Year;
        if (anioPublicacion.HasValue
            && (anioPublicacion.Value < AnioMinimoPublicacion || anioPublicacion.Value > anioActual))
        {
            errores["AnioPublicacion"] = [$"El año de publicación debe estar entre {AnioMinimoPublicacion} y {anioActual}."];
        }

        return errores;
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
