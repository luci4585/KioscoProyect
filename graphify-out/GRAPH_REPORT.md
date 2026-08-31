# Graph Report - KioscoProyect  (2026-08-31)

## Corpus Check
- Corpus is ~8,136 words - fits in a single context window. You may not need a graph.

## Summary
- 45 nodes · 64 edges · 9 communities (7 shown, 2 thin omitted)
- Extraction: 84% EXTRACTED · 16% INFERRED · 0% AMBIGUOUS · INFERRED: 10 edges (avg confidence: 0.79)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Business Domain Entities
- Infrastructure & Architecture
- Application Layer & Patterns
- Authentication & User Management
- Testing Strategy
- Project Documents
- Frontend Stack
- Input Validation
- API Documentation

## God Nodes (most connected - your core abstractions)
1. `Capa de Infraestructura` - 8 edges
2. `Venta` - 8 edges
3. `Baja Lógica` - 7 edges
4. `Capa de Aplicación (Casos de Uso)` - 6 edges
5. `Producto` - 6 edges
6. `Arquitectura Hexagonal (Ports and Adapters)` - 5 edges
7. `Núcleo de Dominio` - 5 edges
8. `PostgreSQL` - 4 edges
9. `Usuario` - 4 edges
10. `Stock` - 4 edges

## Surprising Connections (you probably didn't know these)
- `Capa de Infraestructura` --conceptually_related_to--> `Firebase Authentication`  [EXTRACTED]
  SDD_Sistema_Gestion_Kiosco.md → Base_tecnica.md
- `Núcleo de Dominio` --conceptually_related_to--> `Entity Framework Core`  [INFERRED]
  SDD_Sistema_Gestion_Kiosco.md → Base_tecnica.md
- `Núcleo de Dominio` --conceptually_related_to--> `PostgreSQL`  [INFERRED]
  SDD_Sistema_Gestion_Kiosco.md → Base_tecnica.md
- `Capa de Infraestructura` --conceptually_related_to--> `Firebase Storage`  [EXTRACTED]
  SDD_Sistema_Gestion_Kiosco.md → Base_tecnica.md
- `Capa de Infraestructura` --conceptually_related_to--> `Serilog`  [EXTRACTED]
  SDD_Sistema_Gestion_Kiosco.md → Base_tecnica.md

## Hyperedges (group relationships)
- **Componentes de Arquitectura Hexagonal** — sdd_arquitectura_hexagonal, sdd_nucleo_dominio, sdd_capa_aplicacion, sdd_capa_infraestructura, sdd_puertos_y_adaptadores, sdd_dto_pattern, sdd_repository_pattern, sdd_dependency_injection [EXTRACTED 1.00]
- **Estrategia de Testing** — sdd_pruebas_unitarias, sdd_pruebas_controllers, sdd_pruebas_integracion, sdd_pruebas_regresion [EXTRACTED 1.00]
- **Entidades del Dominio de Negocio** — sdd_producto, sdd_rubro, sdd_stock, sdd_venta, sdd_cliente, sdd_proveedor, sdd_caja, sdd_ingreso_mercaderia [EXTRACTED 1.00]

## Communities (9 total, 2 thin omitted)

### Community 0 - "Business Domain Entities"
Cohesion: 0.36
Nodes (11): Baja Lógica, Caja, Cliente, Ganancia Diaria, Ingreso de Mercadería, Monto Final de Caja, Producto, Proveedor (+3 more)

### Community 1 - "Infrastructure & Architecture"
Cohesion: 0.39
Nodes (9): Entity Framework Core, Firebase Storage, PostgreSQL, Serilog, Arquitectura Hexagonal (Ports and Adapters), Capa de Infraestructura, Núcleo de Dominio, Pruebas de Integración (+1 more)

### Community 2 - "Application Layer & Patterns"
Cohesion: 0.33
Nodes (7): ASP.NET Core Web API, C# / .NET 8, Capa de Aplicación (Casos de Uso), Inyección de Dependencias, DTO (Data Transfer Objects), Repository Pattern, APIs REST

### Community 3 - "Authentication & User Management"
Cohesion: 0.53
Nodes (6): Firebase Authentication, Autorización Interna, Flujo Firebase UID → Usuario Interno, Permiso, Rol, Usuario

### Community 4 - "Testing Strategy"
Cohesion: 0.67
Nodes (4): Ciclo Incremental de Desarrollo, Pruebas de Controllers, Pruebas de Regresión, Pruebas Unitarias

### Community 5 - "Project Documents"
Cohesion: 0.67
Nodes (3): Base Técnica - Sistema de Gestión de Kiosco, Roadmap de Implementación - Sistema de Gestión de Kiosco, SDD - Sistema de Gestión de Kiosco

### Community 6 - "Frontend Stack"
Cohesion: 0.67
Nodes (3): Next.js, React, TypeScript

## Knowledge Gaps
- **10 isolated node(s):** `Base Técnica - Sistema de Gestión de Kiosco`, `Roadmap de Implementación - Sistema de Gestión de Kiosco`, `Repository Pattern`, `TypeScript`, `React` (+5 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 10 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Capa de Infraestructura` connect `Infrastructure & Architecture` to `Authentication & User Management`?**
  _High betweenness centrality (0.384) - this node is a cross-community bridge._
- **Why does `Firebase Authentication` connect `Authentication & User Management` to `Infrastructure & Architecture`?**
  _High betweenness centrality (0.338) - this node is a cross-community bridge._
- **Are the 3 inferred relationships involving `Baja Lógica` (e.g. with `Caja` and `Stock`) actually correct?**
  _`Baja Lógica` has 3 INFERRED edges - model-reasoned connections that need verification._
- **Are the 4 inferred relationships involving `Capa de Aplicación (Casos de Uso)` (e.g. with `Inyección de Dependencias` and `DTO (Data Transfer Objects)`) actually correct?**
  _`Capa de Aplicación (Casos de Uso)` has 4 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Base Técnica - Sistema de Gestión de Kiosco`, `Roadmap de Implementación - Sistema de Gestión de Kiosco`, `Repository Pattern` to the rest of the system?**
  _10 weakly-connected nodes found - possible documentation gaps or missing edges._