namespace Biblioteca.Web.ApiClients;

/// <summary>
/// Cómo terminó una llamada a la Api. El controller decide qué mostrar según cada estado.
/// </summary>
public enum EstadoRespuestaApi
{
    Ok,              // 200 / 201 / 204
    NoEncontrado,    // 404
    Invalido,        // 400
    IsbnDuplicado,   // 409
    ApiNoDisponible  // la Api no respondió (apagada, puerto equivocado, etc.)
}

/// <summary>
/// Lo que devuelven los métodos de los clientes de la Api.
/// Tiene la misma forma que Resultado&lt;T&gt; de la Api: en vez de lanzar excepciones, el
/// estado dice qué pasó y el controller decide con un switch.
/// </summary>
public class RespuestaApi<T>
{
    public EstadoRespuestaApi Estado { get; }

    /// <summary>
    /// El valor devuelto por la Api. Solo tiene sentido cuando Estado es Ok, y puede ser null
    /// si la Api respondió sin cuerpo (por ejemplo, 204 al eliminar).
    /// </summary>
    public T? Valor { get; }

    /// <summary>
    /// Errores por campo (por ejemplo "Isbn" → mensajes). Solo tiene datos cuando Estado es
    /// Invalido o IsbnDuplicado. Las claves son los nombres de las propiedades de los DTOs,
    /// así el controller los puede copiar tal cual a ModelState.
    /// </summary>
    public Dictionary<string, string[]> Errores { get; }

    // Constructor privado: las respuestas se crean con los métodos estáticos de abajo,
    // así no se puede armar una incoherente (por ejemplo, Ok con errores).
    private RespuestaApi(EstadoRespuestaApi estado, T? valor, Dictionary<string, string[]> errores)
    {
        Estado = estado;
        Valor = valor;
        Errores = errores;
    }

    public static RespuestaApi<T> Ok(T? valor)
    {
        return new RespuestaApi<T>(EstadoRespuestaApi.Ok, valor, new Dictionary<string, string[]>());
    }

    public static RespuestaApi<T> NoEncontrado()
    {
        return new RespuestaApi<T>(EstadoRespuestaApi.NoEncontrado, default, new Dictionary<string, string[]>());
    }

    public static RespuestaApi<T> Invalido(Dictionary<string, string[]> errores)
    {
        return new RespuestaApi<T>(EstadoRespuestaApi.Invalido, default, errores);
    }

    public static RespuestaApi<T> IsbnDuplicado(Dictionary<string, string[]> errores)
    {
        return new RespuestaApi<T>(EstadoRespuestaApi.IsbnDuplicado, default, errores);
    }

    public static RespuestaApi<T> ApiNoDisponible()
    {
        return new RespuestaApi<T>(EstadoRespuestaApi.ApiNoDisponible, default, new Dictionary<string, string[]>());
    }
}
