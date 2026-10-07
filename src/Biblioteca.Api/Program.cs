using Biblioteca.Api.Datos;
using Biblioteca.Api.Libros;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Base de datos: este es el ÚNICO lugar donde se elige el proveedor (SQL Server LocalDB).
// La cadena de conexión está en appsettings.Development.json. Los tests la reemplazan
// por SQLite en memoria (ver BibliotecaApiFactory).
string cadenaConexion = builder.Configuration.GetConnectionString("Biblioteca")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Biblioteca' en appsettings.");
builder.Services.AddDbContext<BibliotecaDbContext>(opciones => opciones.UseSqlServer(cadenaConexion));

// Reloj del servidor para RN-05 ("año actual"). Se registra para que los tests lo puedan cambiar.
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<LibroService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
