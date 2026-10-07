# Guía de instalación

Pasos para dejar el proyecto funcionando en un equipo nuevo con **Windows**, tal como lo
usamos: Visual Studio para ver y depurar el código, y Claude Code para desarrollar con IA.

Sigue los pasos **en orden**. Cada uno termina con una comprobación: si no sale lo esperado,
no sigas y mira [Problemas frecuentes](#11-problemas-frecuentes).

> Versiones con las que se probó esta guía: Windows 11, Visual Studio Community 2026 (18.10),
> SDK de .NET 10.0.401, SQL Server Express LocalDB 2025 (17.0), Git 2.55 y Claude Code 2.1.

## Índice

1. [Qué necesitas](#1-qué-necesitas)
2. [Instalar Git](#2-instalar-git)
3. [Instalar Visual Studio (con .NET 10 y LocalDB)](#3-instalar-visual-studio-con-net-10-y-localdb)
4. [Instalar la herramienta de migraciones `dotnet-ef`](#4-instalar-la-herramienta-de-migraciones-dotnet-ef)
5. [Instalar Claude Code](#5-instalar-claude-code)
6. [Clonar el proyecto](#6-clonar-el-proyecto)
7. [Crear la base de datos](#7-crear-la-base-de-datos)
8. [Compilar y correr los tests](#8-compilar-y-correr-los-tests)
9. [Ejecutar el proyecto](#9-ejecutar-el-proyecto)
10. [Trabajar con Claude Code](#10-trabajar-con-claude-code)
11. [Problemas frecuentes](#11-problemas-frecuentes)

---

## 1. Qué necesitas

- Un equipo con **Windows 10 (versión 1809 o posterior) u 11**, 64 bits.
- Unos **20 GB libres** en disco (sobre todo para Visual Studio).
- Conexión a internet.
- Una **cuenta de GitHub** (gratis) para poder subir cambios y crear pull requests. Para
  descargar el código no hace falta: el repositorio es público.
- Una **cuenta de Claude Pro** (o superior) para usar Claude Code. El plan gratuito de
  claude.ai **no** incluye Claude Code.

Casi todos los comandos de esta guía se escriben en **PowerShell**. Para abrirla: menú Inicio →
escribe `PowerShell` → *Windows PowerShell*. Sabrás que estás en PowerShell porque la línea
empieza con `PS C:\...>`.

---

## 2. Instalar Git

Git sirve para descargar el proyecto y guardar los cambios. Claude Code también lo usa: con
Git instalado puede ejecutar comandos de Bash (Git Bash).

1. Descarga **Git for Windows** desde https://git-scm.com/downloads/win e instálalo. Las
   opciones que vienen marcadas por defecto sirven.
2. Cierra y vuelve a abrir PowerShell, y configura tu nombre y correo (aparecen en cada
   commit que hagas):

   ```powershell
   git config --global user.name "Tu Nombre"
   git config --global user.email "tu-correo@ejemplo.com"
   ```

**Comprobación:**

```powershell
git --version
```

Debe mostrar algo como `git version 2.55.0.windows.1`.

---

## 3. Instalar Visual Studio (con .NET 10 y LocalDB)

El proyecto usa **.NET 10**, que necesita **Visual Studio 2026** (versión 18 o posterior).
La edición **Community** es gratuita y sirve.

1. Descarga el instalador desde https://visualstudio.microsoft.com/es/downloads/ (Visual
   Studio 2026 Community).
2. En la pestaña **Cargas de trabajo**, marca **Desarrollo de ASP.NET y web**.
   Esta carga de trabajo instala el SDK de .NET 10, que es lo que compila el proyecto.
3. En la pestaña **Componentes individuales**, busca `LocalDB` y comprueba que
   **SQL Server Express LocalDB** esté marcado. Es la base de datos que usa la app; si no
   está marcado, márcalo.
4. Pulsa **Instalar** y espera a que termine (puede tardar bastante).
5. Al terminar, cierra y vuelve a abrir PowerShell para que reconozca los programas nuevos.

**Comprobación** (en PowerShell):

```powershell
dotnet --list-sdks
sqllocaldb info
```

- `dotnet --list-sdks` debe mostrar un SDK **10.0.4xx o posterior** (por ejemplo,
  `10.0.401 [C:\Program Files\dotnet\sdk]`). El proyecto pide como mínimo la 10.0.401
  (archivo `global.json`).
- `sqllocaldb info` debe mostrar `MSSQLLocalDB`.

Si alguna de las dos falla, vuelve a abrir el instalador de Visual Studio (menú Inicio →
*Visual Studio Installer* → *Modificar*) y revisa los pasos 2 y 3.

---

## 4. Instalar la herramienta de migraciones `dotnet-ef`

`dotnet-ef` crea y actualiza la base de datos a partir de las *migraciones* del proyecto
(la carpeta `src/Biblioteca.Api/Migrations`).

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

Se instala la versión 10.0.12 porque es la misma que usan los paquetes de Entity Framework
del proyecto.

**Comprobación:**

```powershell
dotnet ef --version
```

Debe mostrar `Entity Framework Core .NET Command-line Tools` y `10.0.12`.

---

## 5. Instalar Claude Code

Claude Code es el asistente de IA con el que desarrollamos el proyecto. Se usa desde la
terminal.

1. En PowerShell (no hace falta abrirla como administrador), ejecuta:

   ```powershell
   irm https://claude.ai/install.ps1 | iex
   ```

2. Cuando termine, **cierra PowerShell y abre una nueva**.
3. Comprueba la instalación:

   ```powershell
   claude --version
   ```

   Debe mostrar un número de versión, por ejemplo `2.1.293 (Claude Code)`. Si dice que
   `claude` no se reconoce, mira [Problemas frecuentes](#11-problemas-frecuentes).

4. Inicia sesión: ejecuta `claude`, elige iniciar sesión con tu **cuenta de Claude**
   (suscripción Pro) y sigue los pasos en el navegador. Al terminar, vuelves a la terminal
   con Claude Code abierto. Para salir, escribe `/exit`.

Claude Code se actualiza solo. Si algo falla, `claude doctor` muestra un diagnóstico de la
instalación.

Documentación oficial: https://code.claude.com/docs/en/setup

---

## 6. Clonar el proyecto

Elige una carpeta para tus proyectos (en esta guía, `C:\dev-repo`) y descarga el código:

```powershell
mkdir C:\dev-repo
cd C:\dev-repo
git clone https://github.com/arivaldan/biblioteca.git
cd biblioteca
```

**Comprobación:** en la carpeta `C:\dev-repo\biblioteca` debe estar el archivo
`Biblioteca.slnx` (la solución) y las carpetas `src`, `tests` y `docs`.

> Para **subir cambios** (push) al repositorio necesitas permiso de escritura: pide al dueño
> del repositorio que te agregue como colaborador en GitHub.

---

## 7. Crear la base de datos

La Api guarda los libros en una base de datos SQL Server LocalDB llamada `Biblioteca`. La
base **no se crea sola** al arrancar: hay que crearla una vez con las migraciones.

Desde la carpeta del proyecto (`C:\dev-repo\biblioteca`):

```powershell
dotnet ef database update --project src/Biblioteca.Api
```

La primera vez tarda un poco porque compila el proyecto. La cadena de conexión está en
`src/Biblioteca.Api/appsettings.Development.json`; no hace falta tocarla.

**Comprobación:**

```powershell
dotnet ef migrations list --project src/Biblioteca.Api
```

Al final debe aparecer la migración `..._Inicial` (sin la palabra `(Pending)` al lado).

> Cada vez que alguien agregue una migración nueva (por ejemplo, al crear Socio), hay que
> volver a ejecutar `dotnet ef database update --project src/Biblioteca.Api` después de
> traer los cambios con `git pull`.

---

## 8. Compilar y correr los tests

```powershell
dotnet build
dotnet test
```

**Comprobación:**

- `dotnet build` termina con **0 advertencias y 0 errores**. El proyecto trata cualquier
  advertencia como error (ver `CLAUDE.md`), así que si aparece una, el build falla.
- `dotnet test` termina con todos los tests correctos (`Con error: 0`) en los dos proyectos de
  tests: `Biblioteca.Api.Tests` y `Biblioteca.Web.Tests`. Los tests no usan LocalDB: usan una
  base SQLite en memoria.

---

## 9. Ejecutar el proyecto

La solución tiene dos aplicaciones que deben estar corriendo **a la vez**:

| Aplicación | Qué es | Dirección |
|------------|--------|-----------|
| `Biblioteca.Api` | La Api (datos y reglas de negocio) | http://localhost:5211 (Swagger en http://localhost:5211/swagger) |
| `Biblioteca.Web` | La web que usan las personas | http://localhost:5047 |

La Web no tiene base de datos propia: todo se lo pide a la Api. Si la Api no está corriendo,
la Web muestra el aviso "No se pudo conectar con el servidor de la biblioteca (Api)".

### Opción A: desde Visual Studio (un solo F5)

La primera vez hay que crear un **perfil de arranque múltiple**. Es local: se guarda en
`biblioteca.slnLaunch.user`, que no se sube al repositorio, y por eso cada desarrollador crea
el suyo.

1. Abre Visual Studio → *Abrir un proyecto o una solución* → elige
   `C:\dev-repo\biblioteca\Biblioteca.slnx`.
2. En el **Explorador de soluciones**, clic derecho sobre la solución (la primera línea) →
   **Configurar proyectos de inicio…**
3. Elige **Varios proyectos de inicio**.
4. En la columna *Acción*, pon **Iniciar** en `Biblioteca.Api` y en `Biblioteca.Web`. Deja los
   proyectos de tests en *Ninguno*.
5. Si aparece una columna para elegir el perfil (*Destino de depuración*), elige **http** en
   los dos.
6. Pulsa **Aceptar**. No marques *Compartir perfil*: el proyecto no guarda este perfil en el
   repositorio.
7. Pulsa **F5** (o el botón verde de iniciar). Se abren dos ventanas del navegador: Swagger de
   la Api y la Web.

Las próximas veces basta con abrir la solución y pulsar F5.

### Opción B: desde la terminal (dos terminales)

Abre **dos** PowerShell en `C:\dev-repo\biblioteca`:

```powershell
# Terminal 1: la Api
dotnet run --project src/Biblioteca.Api
```

```powershell
# Terminal 2: la Web
dotnet run --project src/Biblioteca.Web
```

Cada una queda ocupada mientras la aplicación corre. Para detenerlas, pulsa `Ctrl + C` en
cada terminal.

### Comprobación final

1. Abre http://localhost:5047. Debe verse la portada con el menú lateral a la izquierda.
2. Entra en **Libros** (menú lateral) → **Nuevo libro**. Se abre un formulario en una ventana
   (modal).
3. Crea un libro, por ejemplo: título `Rayuela`, autor `Julio Cortázar`, ISBN `8437604947`,
   año `1963`, ejemplares `2`. Al guardar debe aparecer un mensaje de éxito y el libro en el
   listado.
4. Prueba eliminarlo: debe pedir confirmación antes de borrar.

Si todo eso funciona, el ambiente está listo.

---

## 10. Trabajar con Claude Code

Abre una terminal en la carpeta del proyecto y ejecuta `claude`:

```powershell
cd C:\dev-repo\biblioteca
claude
```

Claude Code lee automáticamente `CLAUDE.md`, que tiene las reglas del proyecto (arquitectura,
convenciones, qué está prohibido). La primera vez te preguntará si confías en la carpeta:
responde que sí.

Archivos que conviene leer antes de empezar:

- **`CLAUDE.md`**: cómo está organizado el código y las reglas para escribirlo.
- **`docs/estado-actual.md`**: qué está hecho y cuál es la próxima fase. Se actualiza al
  terminar cada fase.
- **`docs/specs/`**: una spec por funcionalidad (`libro.md`, `libro-web.md`) y la plantilla
  `_plantilla.md` para las nuevas.

Cómo trabajamos (el mismo flujo para cada funcionalidad):

1. **Rama nueva** desde `main` (por ejemplo, `feature/socio-api`).
2. **Spec** en `docs/specs/` a partir de la plantilla. Claude propone las preguntas abiertas;
   tú las respondes. No se escribe código hasta que la spec está aprobada.
3. **Plan** en *plan mode* (Claude propone los pasos y tú los apruebas).
4. **Pasos pequeños**: después de cada paso, Claude compila, corre los tests, hace el commit y
   **se detiene** para que lo revises.
5. **Push y pull request** en GitHub; después del merge, se borra la rama.

Para crear una entidad nueva (Socio, Préstamo…) usa la skill del proyecto:

```text
/nueva-entidad Socio
```

La skill (`.claude/skills/nueva-entidad/SKILL.md`) le indica a Claude los pasos, qué archivos
de Libro usar como molde y lo aprendido hasta ahora.

Algunos comandos útiles dentro de Claude Code:

| Comando | Para qué |
|---------|----------|
| `/help` | Ver la ayuda |
| `/clear` | Empezar una conversación nueva (Claude olvida la anterior) |
| `Shift + Tab` | Cambiar de modo (por ejemplo, entrar en *plan mode*) |
| `/exit` | Salir |

> El archivo `.claude/settings.json` del proyecto define qué comandos puede ejecutar Claude sin
> preguntar y cuáles siempre preguntan (por ejemplo, `git commit` y `git push`). Tus permisos
> personales van en `.claude/settings.local.json`, que no se sube al repositorio.

---

## 11. Problemas frecuentes

**`claude` no se reconoce como comando**
Cierra todas las ventanas de PowerShell y abre una nueva. Si sigue fallando, consulta
https://code.claude.com/docs/en/troubleshoot-install

**`dotnet --list-sdks` no muestra un SDK 10**, o el build dice que no encuentra el SDK
`10.0.401`: abre *Visual Studio Installer* → *Actualizar* (o *Modificar* y revisa la carga de
trabajo *Desarrollo de ASP.NET y web*). Puedes tener un SDK más nuevo (por ejemplo,
`10.0.5xx`): el proyecto lo acepta.

**`sqllocaldb info` no muestra `MSSQLLocalDB`**, o `dotnet ef database update` dice que no
puede conectar con el servidor: falta LocalDB. Instálalo desde *Visual Studio Installer* →
*Modificar* → *Componentes individuales* → **SQL Server Express LocalDB**.

**`dotnet ef` no se reconoce**: repite el paso 4 y abre una PowerShell nueva.

**La Web muestra "No se pudo conectar con el servidor de la biblioteca (Api)"**: la Api no
está corriendo. Arráncala (paso 9). La Web busca la Api en `http://localhost:5211`
(configurado en `src/Biblioteca.Web/appsettings.json`, clave `Api:UrlBase`).

**Error "address already in use" o "puerto en uso" al arrancar**: ya hay otra copia de la Api
o de la Web corriendo (por ejemplo, en otra terminal o en Visual Studio). Ciérrala y vuelve a
intentarlo.

**Al hacer commit, Git avisa `LF will be replaced by CRLF`**: es normal en Windows y no
afecta al código. Puedes ignorarlo.

**Visual Studio no muestra el perfil de arranque múltiple**: créalo de nuevo con los pasos de
la opción A. Es local de cada equipo y no viene con el repositorio.
