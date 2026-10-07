using Biblioteca.Api.Libros;

namespace Biblioteca.Api.Tests.Libros;

public class IsbnTests
{
    // --- RN-07: normalización ---------------------------------------------

    [Theory]
    [InlineData("978-84-376-0494-7", "9788437604947")]
    [InlineData("978 84 376 0494 7", "9788437604947")]
    [InlineData("978-84-376 0494-7", "9788437604947")]
    [InlineData("9788437604947", "9788437604947")]
    public void Normalizar_QuitaGuionesYEspacios(string entrada, string esperado)
    {
        Assert.Equal(esperado, Isbn.Normalizar(entrada));
    }

    [Fact]
    public void Normalizar_XMinuscula_PasaAMayuscula()
    {
        Assert.Equal("080442957X", Isbn.Normalizar("0-8044-2957-x"));
    }

    // --- RN-03: formato -----------------------------------------------------

    [Theory]
    [InlineData("9788437604947")] // ISBN-13
    [InlineData("8437604947")]    // ISBN-10 terminado en dígito
    [InlineData("080442957X")]    // ISBN-10 terminado en X
    public void TieneFormatoValido_FormatoCorrecto_DevuelveTrue(string isbnNormalizado)
    {
        Assert.True(Isbn.TieneFormatoValido(isbnNormalizado));
    }

    [Theory]
    [InlineData("")]               // vacío
    [InlineData("843760494")]      // 9 caracteres
    [InlineData("84376049471")]    // 11 caracteres
    [InlineData("97884376049470")] // 14 caracteres
    [InlineData("0804X29571")]     // X en el medio de un ISBN-10
    [InlineData("978843760494X")]  // X al final de un ISBN-13
    [InlineData("ABCDEFGHIJ")]     // letras
    [InlineData("080442957x")]     // x minúscula sin normalizar
    [InlineData("84376049٤7")]     // dígito árabe, no ASCII
    public void TieneFormatoValido_FormatoIncorrecto_DevuelveFalse(string isbnNormalizado)
    {
        Assert.False(Isbn.TieneFormatoValido(isbnNormalizado));
    }
}
