# AGENTS.md — Sistema de Gestión de Kiosco

## Estado del Proyecto

Proyecto en **implementación activa**. Actualmente en **Etapa 0 (Preparación)** del RoadMap. El backend tiene estructura hexagonal, entidades de dominio, configuraciones EF Core y un controller de prueba. El frontend tiene estructura Next.js con Firebase Auth básico.

## Qué es este sistema

Sistema de gestión integral para kioscos (autoservicios):
- **Backend:** C# / .NET 9 / ASP.NET Core Web API
- **Frontend:** Next.js 16 / TypeScript / React 19 / Tailwind CSS 4
- **Base de datos:** PostgreSQL (local, nativo en Windows)
- **Auth:** Firebase Authentication (frontend) + Firebase Admin SDK (backend, pendiente)
- **Storage:** Firebase Storage (imágenes de productos, pendiente)
- **Arquitectura:** Hexagonal (Ports and Adapters)

## Documentos clave

| Archivo | Propósito |
|---------|-----------|
| `SDD_Sistema_Gestion_Kiosco.md` | **Referencia principal.** 79 requerimientos funcionales (RF-001 a RF-079), 72 requerimientos no funcionales, especificación completa de arquitectura, estrategia de testing. |
| `RoadMap.md` | 18 etapas de implementación (Etapa 0-17). Seguir estrictamente este orden. |
| `Base_tecnica.md` | Resumen del stack tecnológico. |

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
└── Kiosco.Tests/           # UnitTests, ControllerTests, IntegrationTests

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

Seguir estrictamente las etapas 0-17 del `RoadMap.md`. Cada etapa debe pasar tests antes de avanzar. Cadena de dependencias clave:

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

Cuando una tarea involucre Firebase, utiliza preferentemente el MCP oficial de Firebase para inspeccionar o gestionar recursos en lugar de asumir configuraciones.

El MCP no modifica el alcance de las fases del roadmap: disponer de una herramienta Firebase no autoriza a implementar servicios Firebase antes de la fase correspondiente.

Nunca inventes valores de configuración Firebase ni almacenes credenciales o secretos en el repositorio.
