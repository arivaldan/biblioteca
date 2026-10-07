using Biblioteca.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Datos;

/// <summary>
/// Acceso a la base de datos con EF Core. No sabe qué proveedor usa (SQL Server o SQLite):
/// eso se elige en Program.cs (y en los tests), y llega por las opciones del constructor.
/// </summary>
public class BibliotecaDbContext : DbContext
{
    public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Libro> Libros => Set<Libro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Libro>(libro =>
        {
            libro.Property(l => l.Titulo).IsRequired().HasMaxLength(200);
            libro.Property(l => l.Autor).IsRequired().HasMaxLength(150);

            // 13 = largo de un ISBN-13 ya normalizado (sin guiones ni espacios).
            libro.Property(l => l.Isbn).IsRequired().HasMaxLength(13);

            // RN-04: el servicio revisa duplicados antes de guardar; el índice único es la red
            // de seguridad por si dos requests llegan a la vez (ver "Limitaciones conocidas").
            libro.HasIndex(l => l.Isbn).IsUnique();
        });
    }
}
