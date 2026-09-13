# AGENTS.md — Sistema de Gestión de Kiosco

## Project Status

Documentation-only repo. **No code yet.** The repo contains the complete system design before implementation begins. When code arrives, follow `RoadMap.md` Etapa 0-17 strictly.

## What This Is

A kiosco (convenience store) management system with:
- **Backend:** C# / .NET 8 / ASP.NET Core Web API
- **Frontend:** Next.js / TypeScript / React / Tailwind CSS
- **Database:** PostgreSQL
- **Auth:** Firebase Authentication (frontend) + Firebase Admin SDK (backend validation)
- **Storage:** Firebase Storage (product images)
- **Architecture:** Hexagonal (Ports and Adapters)

## Key Documents

| File | Purpose |
|------|---------|
| `SDD_Sistema_Gestion_Kiosco.md` | **Primary reference.** 79 functional requirements (RF-001 to RF-079), 72 non-functional requirements, full architecture spec, testing strategy. |
| `RoadMap.md` | 18 implementation stages (Etapa 0-17). Follow this order. |
| `Base_tecnica.md` | Stack technology summary. |
| `graphify-out/` | Auto-generated knowledge graph of the codebase. Ignore unless querying architecture relationships. |

## Architecture Rules (from SDD)

- Hexagonal architecture: Domain → Application → Infrastructure → API
- Domain must NOT depend on: PostgreSQL, EF Core, Firebase, Serilog, ASP.NET Core
- Dependencies point inward toward the nucleus
- Controllers contain no business logic
- Use DTOs for all API transport (never expose EF entities directly)
- Use FluentValidation for input validation (separate from controllers)
- IDs are `int` autoincremental (PostgreSQL Identity Columns), never Guid/UUID
- All deletions are logical (soft delete), never physical

## Entity Structure

```
Usuario, Rol, Permiso, Rubro, Producto, Cliente, Proveedor,
Stock, Venta, DetalleVenta, Caja, MovimientoCaja,
IngresoMercaderia, DetalleIngresoMercaderia
```

## Planned Project Structure

```
Backend/
├── Kiosco.Domain/          # Entities, ValueObjects, Enums, Exceptions, Interfaces
├── Kiosco.Application/     # DTOs, Interfaces, Services, UseCases, Validators, Mappings
├── Kiosco.Infrastructure/  # Persistence (EF Core), Firebase, Logging (Serilog)
├── Kiosco.API/             # Controllers, Middleware, Extensions, Program.cs
└── Kiosco.Tests/           # UnitTests, ControllerTests, IntegrationTests
```

## Auth Flow

```
Next.js → Firebase Auth SDK → Firebase ID Token
    → Authorization: Bearer <token>
    → ASP.NET Core → Firebase Admin SDK validates token
    → Firebase UID → Internal User (PostgreSQL) → Roles → Permissions
```

## Implementation Order

Follow `RoadMap.md` stages 0-17 strictly. Each stage must pass tests before advancing. Key dependency chain:

```
Preparation → DB Design → Backend Architecture → Auth → Rubros → Productos
→ Clientes/Proveedores → Stock → Ingreso Mercadería → Ventas → Caja
→ Informes → Integration → Full Testing → Security → Docs → Delivery
```

## Testing Strategy

- **Unit tests:** Domain rules, use cases, services, validators, calculations
- **Controller tests:** In-memory DB provider
- **Integration tests:** Dedicated PostgreSQL test database
- **Regression:** Full test suite after each significant change
- Framework: xUnit or NUnit (TBD), FluentAssertions recommended

## Commands (once code exists)

```bash
# Backend
dotnet build
dotnet test
dotnet run --project Backend/Kiosco.API

# Frontend
npm run dev
npm run build
npm run lint
npm run test
```

## Conventions

- Spanish naming for domain entities (Producto, Venta, Caja, etc.)
- English naming for technical code (services, controllers, etc.)
- Use `appsettings.json` + environment variables for config (never hardcode secrets)
- Serilog for logging, never log passwords/tokens/credentials
- Swagger/OpenAPI for API documentation
- ClosedXML or EPPlus for Excel exports (select one during implementation)

## Firebase MCP

Cuando una tarea involucre Firebase, utiliza preferentemente el MCP oficial de Firebase para inspeccionar o gestionar recursos en lugar de asumir configuraciones.

El MCP no modifica el alcance de las fases del roadmap: disponer de una herramienta Firebase no autoriza a implementar servicios Firebase antes de la fase correspondiente.

Nunca inventes valores de configuración Firebase ni almacenes credenciales o secretos en el repositorio.
