# AGENTS.md — Sistema de Gestión de Kiosco

## Estado del Proyecto

Proyecto en **implementación activa**. Última auditoría de estado: **18/11/2026**.

**Cierres autorizados por el responsable (Etapas 0, 1, 2 y 4 el 15/11/2026; Etapas 5, 6, 7 y 8 el 18/11/2026) — Etapas 0, 1, 2, 4, 5, 6, 7 y 8 cerradas:**

- **Evidencia global de cierre:** `dotnet build` 0 errores, `dotnet test` **187/187 en verde**,
  `npm run lint` y `npm run build` correctos.
- **Etapa 0 (Preparación) ✅ cerrada:** ejecutabilidad verificada — la API arranca y sirve
  Swagger (HTTP 200) y `GET /api/Rubros` responde 401 sin token (protección activa).
- **Etapa 1 (Diseño de BD) ✅ cerrada:** migración `20260913230009_InitialCreate`
  **aplicada en `kiosco_db`** verificado contra `__EFMigrationsHistory` de la BD real.
- **Etapa 2 (Arquitectura backend) ✅ cerrada:** incluye el ítem de validaciones: los
  validadores FluentValidation están registrados y **invocados en los controllers
  existentes** (`RubrosController`, `UsuariosController`, `ProductosController` → 400 con
  detalle). Los validadores de entidades sin controller (Cliente y Proveedor ya invocados
  en la Etapa 6; Venta y Caja) se invocarán al crear sus controllers en las Etapas 9–10,
  según corresponda.
- **Etapa 4 (Rubros) ✅ cerrada (15/11/2026):** API CRUD con validadores invocados y permiso
  `rubros.gestionar`, frontend de alta/modificación/baja lógica, 27 tests de la etapa.
- **Etapa 5 (Productos) ✅ cerrada (18/11/2026):** catálogo completo — alta, consulta,
  búsqueda con filtros (RF-016), modificación, baja lógica, precios/costos y asociación con
  rubros; permisos granulares (`productos.consultar/crear/editar/eliminar`), frontend
  `/productos`, **sin imágenes** (fuera de alcance por decisión del responsable del
  18/11/2026), tests de la etapa en verde (**72/72 en total**) y smoke test E2E de login
  real + CRUD verificado contra Firebase/PostgreSQL. **Cierre autorizado por el responsable
  el 18/11/2026.**
- **Etapa 6 (Clientes y proveedores) ✅ cerrada (18/11/2026):** CRUD de clientes
  (RF-027 a RF-031) y de proveedores (RF-032 a RF-035) con búsquedas (RF-029 nombre/
  documento; razón social/CUIT para proveedores), validadores Create+Update invocados,
  permisos sembrados `clientes.gestionar` / `proveedores.gestionar` (cero migraciones),
  consultas solo autenticación, frontend `/clientes` y `/proveedores`, baja lógica,
  **67 tests nuevos (139/139 en verde)** y smoke test E2E real **22/22 pasos**
  (403 sin permiso, CRUD + búsquedas + 400 de validación + CUIT duplicado + bajas
  lógicas, contra Firebase/PostgreSQL). **Cierre autorizado por el responsable
  el 18/11/2026.**
- **Etapa 7 (Stock) ✅ cerrada (18/11/2026):** control de existencias (RoadMap §10) —
  consulta general y por producto (RF-021), ingreso (RF-022), egreso (RF-023) con bloqueo
  de stock insuficiente (RF-077), alertas de stock mínimo (RF-026), validador de
  movimientos invocado, permiso `stock.gestionar` (sembrado, cero migraciones), frontend
  `/stock` con vista de alertas y formularios de ingreso/egreso, **25 tests nuevos
  (164/164 en verde)** y smoke test E2E real **17/17 pasos** (0+10=10, 10−4=6, egreso
  insuficiente → 400 con stock intacto, alertas 6>5 sin alerta → 3≤5 con alerta,
  403 sin permiso, contra Firebase/PostgreSQL). RF-024/RF-025 (actualización automática
  por ventas) ya operativa en `VentaService`, verificable en la Etapa 9. **Cierre
  autorizado por el responsable el 18/11/2026.**
- **Etapa 8 (Ingreso de mercadería) ✅ cerrada (18/11/2026):** registro de ingresos de
  proveedores (RF-036) con asociación de proveedor (RF-037), detalles con cantidades y
  costos de adquisición (RF-039) e **incremento automático de stock (RF-038 — test
  central: 0+12=12, acumulativo 12+3=15)**, rollback con stock intacto ante proveedor
  inexistente (404), validador nuevo invocado, permiso existente `stock.gestionar`
  (cero migraciones), frontend `/ingresos` con detalle de ingresos y link en el Sidebar,
  **23 tests nuevos (187/187 en verde)** y smoke test E2E real **17/17 pasos** (403 sin
  permiso, validaciones 400, contra Firebase/PostgreSQL). El alta devuelve el ingreso con
  proveedor y productos resueltos (fix aplicado por el propio E2E). **Cierre autorizado
  por el responsable el 18/11/2026.**

**Etapa 5 (Productos) — detalle del cierre (cerrada el 18/11/2026):**
API de productos con búsqueda y filtros (nombre, código, rubro, estado, stock — RF-016),
validadores invocados, permisos granulares (`productos.consultar/crear/editar/eliminar`),
frontend `/productos` completo (alta, consulta, filtros, modificación, baja lógica),
21 tests de la etapa (72/72 en total).

**Etapa actual: Etapa 8 CERRADA (18/11/2026).** Siguiente etapa según `RoadMap.md`:
**Etapa 9 — Ventas** (§12), **autorizada e en curso por el responsable el 18/11/2026**.

**Decisión del responsable (18/11/2026) — especificación de roles:** el **superusuario**
es el rol **Administrador** (acceso completo a todas las funciones del sistema: control de
roles, control de usuarios y gestión general); el rol **vendedor** es el **Empleado**
(acceso a las opciones básicas del sistema). Documentado en el SDD (RF-006 y RNF-032) y en
el RoadMap (§6 Etapa 3 — Actividades).

**Decisión del responsable (18/11/2026) — eliminación de la función de imágenes:** la
función de guardar imágenes de productos queda **fuera del alcance** del sistema: se
eliminó del `RoadMap.md` (actividades y Firebase Storage de la Etapa 5), del
`SDD_Sistema_Gestion_Kiosco.md` (RF-020, RNF-035 a RNF-038 y demás menciones) y del
código (endpoints, puerto `IStorageService`, adaptador, UI y tests). La columna
`ImagenUrl` permanece inerte en la BD sin migración. Los pasos de imagen del smoke test
E2E (B8–B11) quedan anulados por cambio de alcance, no pendientes.

**Estado del build (18/11/2026):** `dotnet build` (rebuild completo) queda con **1
advertencia preexistente CS0618** (`Program.cs:65`, código de la Etapa 3,
`GoogleCredential.FromJson`) **dejada a propósito** por decisión del responsable: no afecta
el funcionamiento ni es error. Los "0 advertencias" históricos corresponden a builds
incrementales; la advertencia ya existía en el HEAD `c6828b6` (verificado con build de
revisión aislado) — es ajena a la eliminación de imágenes.

> Los porcentajes de esta sección son **estimaciones de la auditoría del 15/11/2026**,
> recalculadas tras los cierres de las Etapas 0–4. Metodología: se toman las tareas
> documentadas en `RoadMap.md` (actividades + ítems de testing de cada etapa) y se
> clasifican con los estados de la sección "Registro de avance"; ✅ cuentan 1,
> 🟡 cuentan 0,5, 🔵/🔴/⚪ cuentan 0. **No son valores permanentes**: solo reflejan el
> estado en esa fecha y deben recalcarse en la próxima auditoría autorizada.

**Situación por etapa (al 15/11/2026):**

- **Etapa 0 (Preparación): ✅ CERRADA (15/11/2026).**
- **Etapa 1 (Diseño de BD): ✅ CERRADA (15/11/2026).**
- **Etapa 2 (Arquitectura backend): ✅ CERRADA (15/11/2026).**
- **Etapa 3 (Autenticación y autorización): ✅ CERRADA (15/11/2026).** 24 tests.
- **Etapa 4 (Rubros): ✅ CERRADA (15/11/2026).** 27 tests; 51/51 en total.
- **Etapa 5 (Productos): ✅ CERRADA (18/11/2026).** RoadMap §8 — alta, consulta, búsqueda,
  filtros, modificación, baja lógica, precios, costos, asociación con rubros y tests, **sin
  imágenes** (fuera de alcance por decisión del responsable del 18/11/2026). E2E de login
  real + CRUD verificado; 72/72 tests en verde.
- **Etapa 6 (Clientes y proveedores): ✅ CERRADA (18/11/2026).** RoadMap §9 — CRUD de
  clientes, CRUD de proveedores, búsquedas, baja lógica y tests. E2E real 22/22;
  139/139 tests en verde.
- **Etapa 7 (Stock): ✅ CERRADA (18/11/2026).** RoadMap §10 — consulta, ingreso, egreso,
  stock mínimo, alertas y actualización automática. E2E real 17/17; 164/164 tests en verde.
- **Etapa 8 (Ingreso de mercadería): ✅ CERRADA (18/11/2026).** RoadMap §11 — creación
  de ingresos, asociación con proveedor, cantidades, costos y actualización de stock.
  E2E real 17/17; 187/187 tests en verde.
- **Etapa 9 (Ventas) — EN CURSO (18/11/2026, autorizada):** RoadMap §12.
- **Etapa 10 (Caja):** existe código de servicio adelantado (Caja) **sin API, sin frontend
  y sin tests**. Trabajo fuera de orden: no se considera cerrado ni autoriza a avanzar.
- **Etapas 11 a 17:** no iniciadas.
- **Tests en el proyecto: 187, todos en verde (18/11/2026, tras el cierre de la Etapa 8).**

**Diagnóstico de siguiente tarea:** las **Etapas 6, 7 y 8** están **cerradas
(18/11/2026)**. Según el orden de `RoadMap.md`, corresponde la **Etapa 9 — Ventas**
(§12): crear venta, agregar productos, cantidades, subtotales, total, método de pago,
cliente, usuario vendedor, descuento automático de stock, consulta, detalle, anulación y
tests (cálculos, stock insuficiente, persistencia, stock) — **autorizada por el
responsable el 18/11/2026**. Antes de iniciar cualquier tarea deben consultarse los
requisitos y criterios exactos aplicables en `RoadMap.md` y en el SDD. El cierre de la
etapa requerirá autorización expresa del responsable del proyecto.

Avance global estimado al 15/11/2026 (recalculado tras los cierres de Etapas 0–4):
**~37% estricto / ~47% ponderado** (misma metodología descrita arriba, aplicada a las
18 etapas). Estimación de auditoría, no valor permanente.

## Qué es este sistema

Sistema de gestión integral para kioscos (autoservicios):
- **Backend:** C# / .NET 9 / ASP.NET Core Web API
- **Frontend:** Next.js 16 / TypeScript / React 19 / Tailwind CSS 4
- **Base de datos:** PostgreSQL (local, nativo en Windows)
- **Auth:** Firebase Authentication (frontend) + Firebase Admin SDK (backend)
- **Arquitectura:** Hexagonal (Ports and Adapters)

## Documentos clave

| Archivo | Propósito | Autoridad |
|---------|-----------|-----------|
| `RoadMap.md` | 18 etapas de implementación (Etapa 0-17). | **Autoridad única en el orden de implementación y el alcance por etapa.** |
| `SDD_Sistema_Gestion_Kiosco.md` | **Referencia principal de requisitos.** 78 requerimientos funcionales (RF-001 a RF-079, sin RF-020), 68 requerimientos no funcionales (sin RNF-035 a RNF-038), arquitectura, estrategia de testing, criterios de finalización (Parte VIII). | **Autoridad en requisitos y criterios de aceptación.** |
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
