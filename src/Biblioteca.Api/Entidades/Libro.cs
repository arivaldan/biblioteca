namespace Biblioteca.Api.Entidades;

/// <summary>
/// Libro del catálogo, tal como se guarda en la base de datos.
/// Las restricciones (largos máximos, ISBN único) se configuran en BibliotecaDbContext.
/// </summary>
public class Libro
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    // Si hay varios autores, van en el mismo texto separados por coma (RN-02).
    public string Autor { get; set; } = string.Empty;

    // Siempre se guarda normalizado: sin guiones ni espacios y con la X en mayúscula (RN-07).
    public string Isbn { get; set; } = string.Empty;

    public int? AnioPublicacion { get; set; }

    public int CantidadEjemplares { get; set; }
}
