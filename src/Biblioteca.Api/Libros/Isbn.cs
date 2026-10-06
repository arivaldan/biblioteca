namespace Biblioteca.Api.Libros;

/// <summary>
/// Reglas del ISBN que no necesitan base de datos (RN-03 y RN-07).
/// Están separadas del servicio para poder testearlas de forma aislada.
/// </summary>
public static class Isbn
{
    /// <summary>
    /// RN-07: quita guiones y espacios y pasa la "x" final a mayúscula.
    /// Siempre se normaliza antes de validar, guardar o comparar, para que
    /// "84-376-0494-x" y "843760494X" se traten como el mismo ISBN.
    /// </summary>
    public static string Normalizar(string isbn)
    {
        string sinSeparadores = isbn.Replace("-", "").Replace(" ", "");
        return sinSeparadores.ToUpperInvariant();
    }

    /// <summary>
    /// RN-03: el ISBN (ya normalizado) debe ser un ISBN-13 (13 dígitos) o un
    /// ISBN-10 (9 dígitos + un dígito o "X" al final).
    /// No se valida el dígito de control: queda como mejora futura.
    /// </summary>
    public static bool TieneFormatoValido(string isbnNormalizado)
    {
        if (isbnNormalizado.Length == 13)
        {
            return SonTodosDigitos(isbnNormalizado);
        }

        if (isbnNormalizado.Length == 10)
        {
            string primerosNueve = isbnNormalizado.Substring(0, 9);
            char ultimo = isbnNormalizado[9];
            return SonTodosDigitos(primerosNueve) && (char.IsAsciiDigit(ultimo) || ultimo == 'X');
        }

        return false;
    }

    // Se usa IsAsciiDigit y no IsDigit porque IsDigit acepta dígitos de otros
    // alfabetos (por ejemplo "٣", el 3 árabe), que no son válidos en un ISBN.
    private static bool SonTodosDigitos(string texto)
    {
        foreach (char caracter in texto)
        {
            if (!char.IsAsciiDigit(caracter))
            {
                return false;
            }
        }

        return true;
    }
}
