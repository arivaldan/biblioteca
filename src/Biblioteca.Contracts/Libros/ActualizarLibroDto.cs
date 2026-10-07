using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Contracts.Libros;

/// <summary>
/// Datos para actualizar un libro (PUT /api/libros/{id}). No tiene Id: el único Id que
/// cuenta es el de la ruta.
/// </summary>
/// <remarks>
/// Es casi igual a CrearLibroDto a propósito: se mantienen separados para que cada uno
/// pueda cambiar sin afectar al otro.
/// </remarks>
public class ActualizarLibroDto
{
    // [Required] también rechaza un texto con solo espacios (parte de RN-01).
    [Required(ErrorMessage = "El título es obligatorio.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor es obligatorio.")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ISBN es obligatorio.")]
    public string Isbn { get; set; } = string.Empty;

    public int? AnioPublicacion { get; set; }

    // RN-06. Si el campo no viene en el JSON queda en 0, así que también se rechaza.
    [Range(1, 100, ErrorMessage = "La cantidad de ejemplares debe estar entre 1 y 100.")]
    public int CantidadEjemplares { get; set; }
}
