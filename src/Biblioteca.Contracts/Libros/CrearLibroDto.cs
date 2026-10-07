using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Contracts.Libros;

/// <summary>
/// Datos para crear un libro (POST /api/libros). No tiene Id: lo genera la base de datos.
/// </summary>
/// <remarks>
/// Aquí solo van las validaciones que no dependen de transformar el valor, de la base de
/// datos ni del reloj. El resto (trim, largo máximo, ISBN, año) lo valida LibroService.
/// </remarks>
public class CrearLibroDto
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
    // [Required] no cambia nada en la Api (un int siempre tiene valor), pero da el mensaje en
    // español cuando el campo se deja vacío en el formulario de la Web.
    [Required(ErrorMessage = "La cantidad de ejemplares es obligatoria.")]
    [Range(1, 100, ErrorMessage = "La cantidad de ejemplares debe estar entre 1 y 100.")]
    public int CantidadEjemplares { get; set; }
}
