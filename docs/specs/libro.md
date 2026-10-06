# Spec: Libro

> Estado: aprobada

## Objetivo

Permitir registrar y mantener el catálogo de libros de la biblioteca (alta, consulta,
modificación y baja) a través de la API. Es la base que después usarán los préstamos.

## Fuera de alcance

- Vista MVC en `Biblioteca.Web` (por ahora solo la API).
- Paginación del listado.
- Búsqueda o filtros (por título, autor, ISBN, etc.).
- Préstamos y socios.
- Validar el dígito de control del ISBN (mejora futura).

## Modelo de datos

| Campo              | Tipo    | Obligatorio          | Validación                                                    |
|--------------------|---------|----------------------|---------------------------------------------------------------|
| Id                 | int     | Sí (lo genera la BD) | —                                                             |
| Titulo             | string  | Sí                   | Se recorta (trim); no vacío; máximo 200 caracteres            |
| Autor              | string  | Sí                   | Se recorta (trim); no vacío; máximo 150 caracteres            |
| Isbn               | string  | Sí                   | Se normaliza (RN-07); ISBN-10 o ISBN-13 (RN-03); único (RN-04) |
| AnioPublicacion    | int?    | No                   | Entre 1450 y el año actual (ambos incluidos)                  |
| CantidadEjemplares | int     | Sí                   | Entre 1 y 100 (ambos incluidos)                               |

Los DTOs de entrada (`CrearLibroDto`, `ActualizarLibroDto`) **no** tienen `Id`: el único Id
que cuenta es el de la ruta.

## Reglas de negocio

1. **RN-01** — `Titulo` es obligatorio. Se le quitan los espacios del principio y del final;
   si queda vacío es un error. Después del recorte, tiene como máximo 200 caracteres.
2. **RN-02** — `Autor` es obligatorio. Se le quitan los espacios del principio y del final;
   si queda vacío es un error. Después del recorte, tiene como máximo 150 caracteres.
   Si hay varios autores, se escriben en el mismo texto separados por coma.
3. **RN-03** — Después de normalizarlo (RN-07), el `Isbn` debe ser:
   - ISBN-13: exactamente 13 dígitos, o
   - ISBN-10: exactamente 10 caracteres, los 9 primeros dígitos y el último un dígito o `X`.

   La `X` solo se permite como último carácter de un ISBN-10. No se valida el dígito de control.
4. **RN-04** — No puede haber dos libros con el mismo `Isbn` (comparando el ISBN ya
   normalizado). Aplica al crear y al actualizar; al actualizar, un libro puede conservar su
   propio ISBN. Si el ISBN ya lo tiene otro libro, la API responde 409.
   Un ISBN-10 y un ISBN-13 se consideran siempre distintos, aunque correspondan a la misma obra.
5. **RN-05** — `AnioPublicacion` es opcional; si se informa, debe estar entre 1450 y el año
   actual. El año actual se toma del reloj del servidor (`TimeProvider`); no se aceptan años futuros.
6. **RN-06** — `CantidadEjemplares` es obligatorio y debe estar entre 1 y 100.
7. **RN-07** — Normalización del ISBN: antes de validar, guardar o comparar, se eliminan los
   guiones y espacios, y una `x` minúscula se convierte a `X`. Lo que se guarda es el ISBN
   normalizado.
8. **RN-08** — Se permiten varios libros con el mismo título y autor si su ISBN es distinto
   (por ejemplo, otra edición).
9. **RN-09** — El ISBN de un libro se puede modificar con `PUT`, respetando RN-03, RN-04 y RN-07.

## Endpoints de la API

| Método | Ruta              | Respuesta exitosa                                      | Errores                                 |
|--------|-------------------|--------------------------------------------------------|-----------------------------------------|
| GET    | /api/libros       | 200 OK + lista de libros ordenada por `Titulo` ascendente | —                                    |
| GET    | /api/libros/{id}  | 200 OK + el libro                                      | 404 si no existe                        |
| POST   | /api/libros       | 201 Created + el libro creado + cabecera `Location`    | 400 (validación), 409 (ISBN duplicado)  |
| PUT    | /api/libros/{id}  | 200 OK + el libro actualizado                          | 400 (validación), 404, 409 (ISBN duplicado) |
| DELETE | /api/libros/{id}  | 204 No Content (borrado físico)                        | 404 si no existe                        |

Todos los errores (400, 404 y 409) se devuelven con formato `ProblemDetails`. En los 400 se
indica qué campo falló.

## Criterios de aceptación

**Generales**

- [ ] Se puede crear un libro válido y recibir 201 con el libro creado y la cabecera `Location`
      apuntando a `/api/libros/{id}`.
- [ ] Crear un libro con un campo inválido devuelve 400 con `ProblemDetails` (al menos un test
      por cada regla RN-01 a RN-06).
- [ ] Obtener, actualizar o eliminar un id inexistente devuelve 404 con `ProblemDetails`.
- [ ] El listado devuelve todos los libros, ordenados por `Titulo` ascendente.
- [ ] Actualizar un libro válido devuelve 200 con el libro actualizado.
- [ ] Eliminar un libro existente devuelve 204 y después ya no aparece.

**Textos (RN-01, RN-02, RN-08)**

- [ ] Un `Titulo` o `Autor` con espacios al principio o al final se guarda recortado.
- [ ] Un `Titulo` o `Autor` con solo espacios devuelve 400.
- [ ] Un `Titulo` de 200 caracteres es válido; uno de 201 devuelve 400 (igual con `Autor`: 150 / 151).
- [ ] Se pueden crear dos libros con el mismo título y autor si tienen distinto ISBN.

**ISBN (RN-03, RN-04, RN-07, RN-09)**

- [ ] Un ISBN con guiones o espacios (ej. `978-84-376 0494-7`) se acepta y se guarda sin ellos.
- [ ] Un ISBN-10 terminado en `x` minúscula se acepta y se guarda con `X` mayúscula.
- [ ] Un ISBN con `X` en otra posición que no sea la última, o un ISBN-13 con `X`, devuelve 400.
- [ ] Un ISBN que, normalizado, no tiene 10 ni 13 caracteres devuelve 400.
- [ ] Crear un libro con un ISBN ya registrado devuelve 409 con `ProblemDetails`, también si
      solo se diferencia en guiones o espacios.
- [ ] Actualizar un libro usando el ISBN de *otro* libro devuelve 409.
- [ ] Actualizar un libro manteniendo su propio ISBN funciona (no da 409).
- [ ] Actualizar un libro con un ISBN nuevo y libre funciona y se guarda normalizado.

**Números (RN-05, RN-06)**

- [ ] `AnioPublicacion` igual al año actual es válido; el año siguiente devuelve 400
      (el test fija el año usando un `TimeProvider` falso).
- [ ] `AnioPublicacion` 1449 devuelve 400; 1450 es válido; sin año también es válido.
- [ ] `CantidadEjemplares` 0 y 101 devuelven 400; 1 y 100 son válidos.

## Decisiones

Respuestas a las preguntas que estaban abiertas en el borrador.

1. **Guiones y espacios en el ISBN:** se aceptan en la entrada, pero se eliminan antes de guardar y comparar.
2. **ISBN-10 terminado en X:** se acepta, solo como último carácter; se guarda en mayúscula.
3. **Dígito de control:** no se valida (mejora futura).
4. **ISBN-10 vs. ISBN-13 equivalentes:** se consideran ISBN distintos.
5. **Cambiar el ISBN:** se puede, respetando RN-04.
6. **Espacios en Titulo y Autor:** se recortan (trim); si quedan vacíos, es error 400.
7. **Mismo título y autor:** se permiten si el ISBN es distinto.
8. **Varios autores:** `Autor` es un solo texto; los autores se separan por coma.
9. **Respuesta del PUT:** 200 con el libro actualizado.
10. **Respuesta del POST:** 201 con cabecera `Location`.
11. **Id en el cuerpo del PUT:** los DTOs de entrada no tienen `Id`; solo cuenta el de la ruta.
12. **Orden del listado:** por `Titulo` ascendente.
13. **Formato de errores:** todos (400, 404, 409) usan `ProblemDetails`.
16. **Máximo de ejemplares:** `CantidadEjemplares` entre 1 y 100.
17. **Año actual:** se toma del reloj del servidor (`TimeProvider`); no se aceptan años futuros.

Las preguntas 14 y 15 siguen abiertas (ver abajo). Mientras tanto, el borrado es físico.

## Preguntas abiertas

Se resolverán en la spec de Préstamo:

14. ¿Se podrá eliminar un libro que tenga préstamos (activos o ya devueltos)? ¿Borrado físico
    o borrado lógico (marcarlo como "dado de baja")? *Por ahora el borrado es físico.*
15. ¿Se podrá bajar `CantidadEjemplares` por debajo del número de ejemplares prestados en ese momento?
