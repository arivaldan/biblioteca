namespace Biblioteca.Contracts.Libros;

/// <summary>
/// Libro tal como lo devuelve la API.
/// </summary>
public class LibroDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Autor { get; set; } = string.Empty;

    public string Isbn { get; set; } = string.Empty;

    public int? AnioPublicacion { get; set; }

    public int CantidadEjemplares { get; set; }
}
