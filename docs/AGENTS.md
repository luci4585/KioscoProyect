# AGENTS.md — Sistema de Gestión de Kiosco

## Estado del Proyecto

Proyecto en **implementación activa**. Última auditoría de estado: **15/11/2026**.

**Etapa actual: Etapa 3 — Autenticación y autorización** (~41% de sus tareas implementadas;
ver estimación más abajo).

> Los porcentajes de esta sección son **estimaciones de la auditoría del 15/11/2026**.
> Metodología: se toman las tareas documentadas en `RoadMap.md` (actividades + ítems de
> testing de cada etapa) y se clasifican con los estados de la sección "Registro de avance";
> ✅ cuentan 1, 🟡 cuentan 0,5, 🔵/🔴/⚪ cuentan 0. **No son valores permanentes**: solo
> reflejan el estado en esa fecha y deben recalcarse en la próxima auditoría autorizada.

**Situación por etapa (al 15/11/2026):**

- **Etapa 0 (Preparación):** *implementación completada, etapa NO validada ni cerrada.*
  Hecho: repo Git/GitHub, solución de 5 proyectos, frontend Next.js, PostgreSQL, Firebase,
  variables de entorno, proyecto de tests, Swagger, estructura hexagonal.
  Validación pendiente: ejecutabilidad del proyecto base ("proyecto base ejecutable").
- **Etapa 1 (Diseño de BD):** *implementación completada, etapa NO validada ni cerrada.*
  Hecho: 15 entidades, 15 configuraciones EF Core, índices, Identity Columns, migración
  `InitialCreate`.
  Validación pendiente: migración aplicada en la BD real.
- **Etapa 2 (Arquitectura backend):** *implementación completada con ítems parciales, etapa NO
  validada ni cerrada.*
  Hecho: proyectos, DI, DbContext, PostgreSQL, middleware de errores, DTOs, mappings, Serilog.
  Pendiente: los validadores FluentValidation están registrados pero aún no se invocan desde
  controllers/servicios.
- **Etapa 3 (actual):** hecho — Firebase Auth SDK, login, logout, Firebase ID Token, Firebase
  Admin SDK, usuario interno (`/api/auth/me`), sesión protegida en frontend.
  Pendiente — recuperación de contraseña, aplicación real de roles y permisos en endpoints,
  gestión de usuarios/roles, y los 6 tests de testing de la etapa.
- **Etapa 4 (Rubros):** parcial — API CRUD completa y baja lógica; frontend solo lectura
  (sin alta/edición/baja); sin tests.
- **Etapas 5 a 10:** existe código de servicio adelantado (Producto, Cliente, Proveedor,
  Stock, IngresoMercaderia, Venta, Caja) **sin API, sin frontend y sin tests**. Trabajo fuera
  de orden: no se considera cerrado ni autoriza a avanzar.
- **Etapas 11 a 17:** no iniciadas.
- **Tests en el proyecto: 0 (cero).** `Kiosco.Tests/` solo contiene el `.csproj`.

**Diagnóstico de siguiente tarea (no es autorización para ejecutarla):** según el orden de
`RoadMap.md`, lo que corresponde a continuación es completar la Etapa 3, empezando por su
batería de tests. Antes de iniciar cualquier tarea deben consultarse los requisitos y
criterios exactos aplicables en `RoadMap.md` y en el SDD. La ejecución requiere
autorización expresa del responsable del proyecto.

Avance global estimado al 15/11/2026: **~28% estricto / ~41% ponderado** (misma metodología
descrita arriba, aplicada a las 18 etapas). Estimación de auditoría, no valor permanente.

## Qué es este sistema

Sistema de gestión integral para kioscos (autoservicios):
- **Backend:** C# / .NET 9 / ASP.NET Core Web API
- **Frontend:** Next.js 16 / TypeScript / React 19 / Tailwind CSS 4
- **Base de datos:** PostgreSQL (local, nativo en Windows)
- **Auth:** Firebase Authentication (frontend) + Firebase Admin SDK (backend)
- **Storage:** Firebase Storage (imágenes de productos, pendiente — Etapa 5)
- **Arquitectura:** Hexagonal (Ports and Adapters)

## Documentos clave

| Archivo | Propósito | Autoridad |
|---------|-----------|-----------|
| `RoadMap.md` | 18 etapas de implementación (Etapa 0-17). | **Autoridad única en el orden de implementación y el alcance por etapa.** |
| `SDD_Sistema_Gestion_Kiosco.md` | **Referencia principal de requisitos.** 79 requerimientos funcionales (RF-001 a RF-079), 72 requerimientos no funcionales, arquitectura, estrategia de testing, criterios de finalización (Parte VIII). | **Autoridad en requisitos y criterios de aceptación.** |
| `Base_tecnica.md` | Resumen del stack tecnológico. | Complementario. |

**Consulta obligatoria:** antes de iniciar cualquier tarea se deben leer (o releer) el
`RoadMap.md` (para el orden y el alcance) y los apartados correspondientes del SDD (para los
requisitos y criterios de aceptación de esa etapa). No se trabaja sobre suposiciones.

## Autoridad documental

- El orden de desarrollo lo define **exclusivamente** `docs/RoadMap.md`.
- El alcance funcional de cada etapa lo definen `RoadMap.md` (actividades) y el SDD (RF/RNF).
- Si este archivo AGENTS.md y el roadmap o el SDD dijeran cosas distintas, **vale el
  roadmap/SDD**: este AGENTS.md es solo una guía operativa y debe corregirse (con
  autorización) para realinearse.
- Este archivo no puede crear etapas, reordenarlas ni ampliar su alcance.

## Alcance permitido y cambios que requieren autorización

**Prohibido sin autorización explícita del responsable del proyecto:**
- Implementar funcionalidades que no estén contempladas en la etapa actual según `RoadMap.md` y el SDD.
- Modificar el roadmap, el SDD o la Base técnica.
- Cambiar la arquitectura, el stack o las tecnologías definidas.
- Crear archivos `AGENTS.md` adicionales en otros directorios.
- Refactorizaciones no solicitadas, reorganización de proyectos, eliminación de código o
  cambios "de mejora" que no haya pedido el usuario.
- Avanzar a una etapa posterior antes de cerrar la actual.

**Requiere autorización previa:** cualquier cambio adicional o refactorización que exceda lo
estrictamente pedido en la tarea, aunque parezca mejora evidente. Se propone, se espera
aprobación, se ejecuta.

## Criterios de cierre de etapa

**Diferencia entre etapa implementada y etapa cerrada:** que una etapa tenga su código
escrito **no** significa que esté cerrada. La implementación completada es una condición
necesaria; la validación es la que cierra la etapa.

Una etapa solo se considera **validada y cerrada** cuando **todos** estos puntos se cumplen:

1. Todas las actividades de la etapa están implementadas y son utilizables (no solo
   mencionadas en documentos ni existentes a medias en una capa sin exponer).
2. Los **criterios de aceptación** del SDD aplicables a la etapa están satisfechos.
3. Los **tests** exigidos por la etapa están escritos y **pasan** (`dotnet test` en verde;
   en su caso, suite de frontend correspondiente).
4. No quedan ítems 🔴 ni 🟡 sin justificación explícita y aprobada.
5. La verificación se hace sobre evidencia real (código, ejecución, tests), nunca solo
   porque una tarea figure en un documento.
6. El cierre de la etapa fue **revisado y autorizado** por el responsable del proyecto.

**No se puede avanzar de etapa por iniciativa propia.** El avance lo autoriza el usuario
después de revisar el cierre documentado de la etapa anterior (RNF-065: una funcionalidad no
se considera finalizada con pruebas fallidas relacionadas).

## Registro de avance

El estado del proyecto se registra **únicamente en la sección "Estado del Proyecto" de este
archivo**. Condiciones para actualizarlo:

- **Solo con evidencia verificable** de un cambio real (código inspeccionado, ejecución,
  tests en verde, migración aplicada, etc.), nunca por suposición o por que una tarea figure
  en un documento.
- **Solo con autorización expresa** del responsable del proyecto. No se modifica el estado
  automáticamente.
- Se conserva la fecha de la última auditoría y se indica qué evidencia respalda el cambio.

Estados a usar, con evidencia:

| Estado | Significado |
|--------|-------------|
| ✅ COMPLETADA | Implementada, verificada con evidencia y con sus tests en verde. |
| 🟡 PARCIAL | Implementada pero con aspectos pendientes (detallar cuáles). |
| 🔵 EN VERIFICACIÓN | Aparentemente implementada, falta evidencia para confirmarla. |
| 🔴 PENDIENTE | No implementada dentro de una etapa en curso. |
| ⚪ NO INICIADA | Corresponde a una etapa futura. |

Regla: **no marcar ✅ sin evidencia verificable** (código + ejecución/tests). Las desviaciones
detectadas (trabajo fuera de orden, alcance adelantado) se anotan en el estado de la etapa
correspondiente, sin borrarlas ni justificarlas.

## Contradicciones entre documentación y código

Ante una diferencia entre documentos y código:

1. **No elegir una versión por iniciativa propia.** No modificar el roadmap, el SDD ni el
   código para "resolverla" sin autorización.
2. Registrar la contradicción en la sección "Estado del Proyecto" (o señalarla al usuario
   en la respuesta) indicando: qué dice el documento, qué hace el código y qué etapa queda
   afectada.
3. Esperar la decisión del usuario: corregir el documento, corregir el código, o aceptar la
   diferencia como temporal.
4. Mientras tanto, en dudas de alcance u orden, **prevalece `RoadMap.md`**.

## Reglas de arquitectura (del SDD)

- Arquitectura hexagonal: Domain → Application → Infrastructure → API
- Domain NO debe depender de: PostgreSQL, EF Core, Firebase, Serilog, ASP.NET Core
- Las dependencias apuntan hacia el núcleo
- Los Controllers no contienen lógica de negocio
- Usar DTOs para todo transporte de datos de la API (nunca exponer entidades EF directamente)
- Usar FluentValidation para validación de entrada (separado de controllers)
- IDs son `int` autoincremental (PostgreSQL Identity Columns), nunca Guid/UUID
- Todas las eliminaciones son lógicas (soft delete), nunca físicas

## Estructura de entidades

```
Usuario, Rol, Permiso, RolPermiso, Rubro, Producto, Cliente, Proveedor,
Stock, Venta, DetalleVenta, Caja, MovimientoCaja,
IngresoMercaderia, DetalleIngresoMercaderia
```

## Estructura del proyecto

```
Backend/
├── Kiosco.Domain/          # Entidades, ValueObjects, Enums, Excepciones, Interfaces
├── Kiosco.Application/     # DTOs, Interfaces, Servicios, UseCases, Validators, Mappings
├── Kiosco.Infrastructure/  # Persistencia (EF Core), Firebase, Logging (Serilog)
├── Kiosco.API/             # Controllers, Middleware, Extensions, Program.cs
└── Kiosco.Tests/           # UnitTests, ControllerTests, IntegrationTests (aún sin crear)

frontend/
├── src/
│   ├── app/                # Next.js App Router
│   ├── components/         # Componentes React reutilizables
│   ├── lib/                # Firebase config, auth helpers
│   ├── services/           # Cliente API REST
│   ├── types/              # Interfaces TypeScript
│   └── hooks/              # Custom hooks
```

## Flujo de autenticación

```
Next.js → Firebase Auth SDK → Firebase ID Token
    → Authorization: Bearer <token>
    → ASP.NET Core → Firebase Admin SDK valida token
    → Firebase UID → Usuario interno (PostgreSQL) → Roles → Permisos
```

## Orden de implementación

Seguir estrictamente las etapas 0-17 del `RoadMap.md`. Cada etapa debe pasar tests antes de
avanzar. **No se salta, ni se repite, ni se inventa una etapa por iniciativa propia.**
Cadena de dependencias clave:

```
Preparación → DB Design → Arquitectura Backend → Auth → Rubros → Productos
→ Clientes/Proveedores → Stock → Ingreso Mercadería → Ventas → Caja
→ Informes → Integración → Testing Completo → Seguridad → Docs → Entrega
```

## Estrategia de testing

- **Tests unitarios:** Reglas de dominio, use cases, servicios, validadores, cálculos
- **Tests de controllers:** Proveedor de DB en memoria
- **Tests de integración:** Base de datos PostgreSQL dedicada de testing
- **Regresión:** Suite completa de tests después de cada cambio significativo
- Framework: xUnit, FluentAssertions (recomendado)
- **Estado actual: no existe ningún test en el proyecto.** Según el orden del roadmap, la
  batería de tests de la Etapa 3 es la siguiente tarea pendiente (diagnóstico, no
  autorización: ver "Estado del Proyecto").

## Comandos

```bash
# Backend
dotnet restore
dotnet build
dotnet test
dotnet run --project Backend/Kiosco.API

# Frontend
cd frontend
npm install
npm run dev
npm run build
npm run lint
```

## Convenciones

- Naming en español para entidades de dominio (Producto, Venta, Caja, etc.)
- Naming en inglés para código técnico (services, controllers, etc.)
- Usar `appsettings.json` + User Secrets + variables de entorno para configuración (nunca hardcodear secretos)
- Serilog para logging, nunca registrar passwords/tokens/credenciales
- Swagger/OpenAPI para documentación de la API
- ClosedXML o EPPlus para exportaciones Excel (seleccionar durante implementación)

## Firebase MCP

Cuando una tarea involucre Firebase, utiliza preferentemente el MCP oficial de Firebase para
inspeccionar o gestionar recursos en lugar de asumir configuraciones.

El MCP no modifica el alcance de las fases del roadmap: disponer de una herramienta Firebase
no autoriza a implementar servicios Firebase antes de la fase correspondiente.

Nunca inventes valores de configuración Firebase ni almacenes credenciales o secretos en el
repositorio.
