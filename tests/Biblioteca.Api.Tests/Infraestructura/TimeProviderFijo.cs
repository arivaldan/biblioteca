namespace Biblioteca.Api.Tests.Infraestructura;

/// <summary>
/// Reloj falso para los tests: siempre devuelve la misma fecha.
/// Así un test del "año actual" no empieza a fallar solo cuando cambia el año.
/// </summary>
public class TimeProviderFijo : TimeProvider
{
    private readonly DateTimeOffset _ahora;

    public TimeProviderFijo(DateTimeOffset ahora)
    {
        _ahora = ahora;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _ahora;
    }

    // Sin esto, GetLocalNow() usaría la zona horaria de la máquina que corre los tests,
    // y el resultado podría cambiar según dónde se ejecuten.
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
