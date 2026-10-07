using Biblioteca.Api.Datos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Tests.Infraestructura;

/// <summary>
/// Base de datos SQLite en memoria para un test. Cada instancia es una base nueva y vacía.
/// Uso: <c>using var bd = new BaseDeDatosEnMemoria();</c>
/// </summary>
public sealed class BaseDeDatosEnMemoria : IDisposable
{
    // Una base SQLite en memoria existe solo mientras su conexión está abierta.
    // Por eso la conexión se abre aquí y se cierra recién en Dispose, al terminar el test.
    private readonly SqliteConnection _conexion;

    public BaseDeDatosEnMemoria()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        // Se usa EnsureCreated y no las migraciones porque las migraciones se generan
        // para SQL Server y podrían no funcionar en SQLite.
        using BibliotecaDbContext contexto = CrearContexto();
        contexto.Database.EnsureCreated();
    }

    /// <summary>
    /// Crea un DbContext nuevo sobre la misma base. Conviene usar uno para preparar los datos
    /// y otro para comprobarlos, así se lee de verdad de la base y no de la memoria de EF.
    /// </summary>
    public BibliotecaDbContext CrearContexto()
    {
        DbContextOptions<BibliotecaDbContext> opciones = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseSqlite(_conexion)
            .Options;

        return new BibliotecaDbContext(opciones);
    }

    public void Dispose()
    {
        _conexion.Dispose();
    }
}
