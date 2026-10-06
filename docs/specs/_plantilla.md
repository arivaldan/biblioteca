# Spec: <Nombre de la entidad o funcionalidad>

> Estado: borrador | en revisión | aprobada

## Objetivo

Qué problema resuelve y para quién, en 2-3 frases.

## Fuera de alcance

Lo que **no** se hace en esta spec (para no agrandar el trabajo sin querer).

- ...

## Modelo de datos

| Campo | Tipo | Obligatorio | Validación |
|-------|------|-------------|------------|
| Id    | int  | Sí (lo genera la BD) | — |
| ...   | ...  | ...         | ...        |

## Reglas de negocio

Cada regla tiene un ID para poder nombrarla en tests y commits. Toda regla necesita al menos un test.

1. **RN-01** — ...
2. **RN-02** — ...

## Endpoints de la API

| Método | Ruta | Respuesta exitosa | Errores |
|--------|------|-------------------|---------|
| GET    | /api/... | 200 OK + ... | ... |

## Criterios de aceptación

- [ ] ...
- [ ] ...

## Decisiones

Preguntas ya resueltas, cada una con su respuesta en una línea. Se conserva el número que
tenía en "Preguntas abiertas" para poder rastrearla.

1. **<Tema>:** <respuesta>.

## Preguntas abiertas

Dudas que hay que resolver **antes** de implementar. No suponer respuestas.
Cuando una se resuelve, se mueve a "Decisiones". Si se deja para otra spec, se indica cuál.

1. ...
