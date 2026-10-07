# Biblioteca

App de préstamo de libros (`Libro`, `Socio`, `Préstamo`) 
apoyado por IA con [Claude Code](https://code.claude.com/docs). prioriza código simple y legible.

## Empezar

**👉 Sigue la [guía de instalación](docs/guia-instalacion.md)**: instalar Visual Studio, .NET 10,
LocalDB y Claude Code, clonar el proyecto, crear la base de datos y ejecutarlo.

Resumen para quien ya tiene el ambiente listo:

```powershell
git clone https://github.com/arivaldan/biblioteca.git
cd biblioteca
dotnet ef database update --project src/Biblioteca.Api   # crea la base de datos (una vez)
dotnet build
dotnet test
dotnet run --project src/Biblioteca.Api                  # terminal 1 → http://localhost:5211/swagger
dotnet run --project src/Biblioteca.Web                  # terminal 2 → http://localhost:5047
```

## Qué hay en la solución

| Proyecto | Qué es |
|----------|--------|
| `src/Biblioteca.Api` | Web API (controllers → services → EF Core con SQL Server LocalDB) |
| `src/Biblioteca.Web` | Web MVC que consume la Api con un `HttpClient` tipado |
| `src/Biblioteca.Contracts` | DTOs compartidos entre Api y Web |
| `tests/Biblioteca.Api.Tests` | Tests de la Api (xUnit, SQLite en memoria) |
| `tests/Biblioteca.Web.Tests` | Tests de la Web (xUnit, sin red) |

## Documentación

- [`docs/guia-instalacion.md`](docs/guia-instalacion.md): instalación paso a paso.
- [`CLAUDE.md`](CLAUDE.md): arquitectura, convenciones y reglas del proyecto (también las lee
  Claude Code).
- [`docs/estado-actual.md`](docs/estado-actual.md): qué está hecho y qué sigue.
- [`docs/specs/`](docs/specs): especificación de cada funcionalidad.
