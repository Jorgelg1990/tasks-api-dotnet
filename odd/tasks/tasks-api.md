# Tasks API (.NET 10)

Objetivo: Implementar una API de tareas en .NET 10 con SQLite, migraciones, validaciones, filtros, paginación, tests y README.
Rama: `main` (inicial) / feature branch según avance. Estrategia de entrega: `ask-on-risk`. Test runner: `dotnet test`.

## Specs

- **S1.** "1. Crear, listar, ver, editar y borrar tareas. Cada tarea tiene título, descripción, estado (pendiente, en curso o hecha), responsable y fecha límite."
  - Alcance: CRUD completo de tareas con campos título, descripción, estado (`pendiente`, `en curso`, `hecha`), responsable y fecha límite.
  - Criterios: crear, listar, ver (detalle), editar y borrar disponibles vía HTTP.
- **S2.** "2. Filtros por estado y por responsable, con paginación."
  - Alcance: listado con filtros por estado y responsable, y paginación.
  - Criterios: parámetros de filtro y paginación funcionan y se validan razonablemente.
- **S3.** "3. Validación de los datos y errores con los códigos HTTP correctos."
  - Alcance: validación de entrada y respuestas de error con códigos HTTP apropiados (400, 404, etc.).
- **S4.** "4. Base de datos SQL Server, PostgreSQL o SQLite con migraciones."
  - Alcance: usar SQLite con Entity Framework Core y migraciones.
  - Criterios: esquema gestionado por migraciones; base persistente local.
- **S5.** "5. Al menos 3 tests."
  - Alcance: mínimo 3 tests automatizados ejecutables con el test runner.
- **S6.** "6. Un README con cómo ejecutarlo, las decisiones que tomaste, qué harías con más tiempo y cómo usaste la IA (qué le pediste, qué aceptaste y qué corregiste)."
  - Alcance: README completo con ejecución, decisiones, mejoras futuras y uso de IA (qué se pidió, qué se aceptó y qué se corrigió).

## Tasks

| ID | Specs | Ruta | Estado | Evidencia (commit/RED-GREEN) |
|---|---|---|---|---|
| T1 | S4 | inline | pendiente | - |
| T2 | S1,S3 | inline | pendiente | - |
| T3 | S2,S3 | inline | pendiente | - |
| T4 | S5 | inline | pendiente | - |
| T5 | S6 | inline | pendiente | - |

## Log

- **L1 (solicitud original, verbatim):**
  1. Crear, listar, ver, editar y borrar tareas. Cada tarea tiene título, descripción, estado (pendiente, en curso o hecha), responsable y fecha límite.
  2. Filtros por estado y por responsable, con paginación.
  3. Validación de los datos y errores con los códigos HTTP correctos.
  4. Base de datos SQL Server, PostgreSQL o SQLite con migraciones.
  5. Al menos 3 tests.
  6. Un README con cómo ejecutarlo, las decisiones que tomaste, qué harías con más tiempo y cómo usaste la IA (qué le pediste, qué aceptaste y qué corregiste).
- **L2 (aclaraciones usuario):** ".net 10" / "si estoy de acuerdo" (acepta carpeta `C:\Users\User\TasksApi` y SQLite por defecto).
- **L3:** Inicio de implementación. Riesgo previsto bajo-medio (API nueva, sin datos existentes); se evaluará con `gentle-ai review assess` por commit de unidad de trabajo cuando corresponda.
