using Biblioteca.Api.Datos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Biblioteca.Api.Tests.Infraestructura;

/// <summary>
/// Levanta la Api completa en memoria (sin servidor real) para hacerle requests HTTP.
/// Cambia dos cosas respecto de Program.cs: SQL Server por SQLite en memoria, y el reloj
/// real por uno fijo. Cada instancia tiene su propia base vacía.
/// Uso: <c>using var api = new BibliotecaApiFactory(); HttpClient cliente = api.CreateClient();</c>
/// </summary>
public class BibliotecaApiFactory : WebApplicationFactory<Program>
{
    public const int AnioActual = 2026;

    // Igual que en BaseDeDatosEnMemoria: la base existe mientras la conexión esté abierta.
    private readonly SqliteConnection _conexion = new SqliteConnection("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _conexion.Open();

        // ConfigureTestServices se ejecuta DESPUÉS de los registros de Program.cs,
        // así que aquí podemos quitar lo que registró Program.cs y poner otra cosa.
        builder.ConfigureTestServices(services =>
        {
            // Para cambiar de proveedor hay que quitar las dos piezas que dejó AddDbContext
            // con SQL Server; si queda alguna, EF se queja de tener dos proveedores.
            services.RemoveAll<DbContextOptions<BibliotecaDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<BibliotecaDbContext>>();
            services.AddDbContext<BibliotecaDbContext>(opciones => opciones.UseSqlite(_conexion));

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(
                new TimeProviderFijo(new DateTimeOffset(AnioActual, 6, 15, 12, 0, 0, TimeSpan.Zero)));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        IHost host = base.CreateHost(builder);

        // Crea las tablas en la base SQLite antes del primer request.
        using IServiceScope scope = host.Services.CreateScope();
        BibliotecaDbContext contexto = scope.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
        contexto.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _conexion.Dispose();
        }
    }
}
