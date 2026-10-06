namespace Biblioteca.Api.Libros;

/// <summary>
/// Cómo terminó una operación del servicio. El controller traduce cada estado a un código HTTP.
/// </summary>
public enum EstadoResultado
{
    Ok,             // 200 / 201 / 204
    NoEncontrado,   // 404
    IsbnDuplicado,  // 409
    Invalido        // 400
}

/// <summary>
/// Lo que devuelve el servicio al crear, actualizar o eliminar.
/// Se usa en lugar de lanzar excepciones para que el flujo sea explícito: mirando la
/// firma del método ya se sabe que puede fallar, y el controller decide con un switch.
/// </summary>
public class Resultado<T>
{
    public EstadoResultado Estado { get; }

    /// <summary>El valor devuelto. Solo tiene sentido cuando Estado es Ok.</summary>
    public T? Valor { get; }

    /// <summary>
    /// Errores de validación por campo (por ejemplo "Isbn" → mensajes).
    /// Solo tiene datos cuando Estado es Invalido. Tiene la misma forma que
    /// ValidationProblemDetails.Errors, así el controller lo puede usar tal cual.
    /// </summary>
    public Dictionary<string, string[]> Errores { get; }

    // Constructor privado: los resultados se crean con los métodos estáticos de abajo,
    // así no se puede armar uno incoherente (por ejemplo, Ok con errores).
    private Resultado(EstadoResultado estado, T? valor, Dictionary<string, string[]> errores)
    {
        Estado = estado;
        Valor = valor;
        Errores = errores;
    }

    public static Resultado<T> Ok(T valor)
    {
        return new Resultado<T>(EstadoResultado.Ok, valor, new Dictionary<string, string[]>());
    }

    public static Resultado<T> NoEncontrado()
    {
        return new Resultado<T>(EstadoResultado.NoEncontrado, default, new Dictionary<string, string[]>());
    }

    public static Resultado<T> IsbnDuplicado()
    {
        return new Resultado<T>(EstadoResultado.IsbnDuplicado, default, new Dictionary<string, string[]>());
    }

    public static Resultado<T> Invalido(Dictionary<string, string[]> errores)
    {
        return new Resultado<T>(EstadoResultado.Invalido, default, errores);
    }
}
