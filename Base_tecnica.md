# SDD — Base Técnica
## Sistema de Gestión de Kiosco

---

## 1. Introducción
La presente Base Técnica define las tecnologías, herramientas, frameworks, servicios y criterios técnicos que serán utilizados para el desarrollo del **Sistema de Gestión de Kiosco**.

El proyecto estará compuesto por un frontend web desarrollado con **Next.js**, **TypeScript**, **React** y **Tailwind CSS**, un backend desarrollado en **C# (.NET 8 o superior)** mediante **ASP.NET Core Web API** bajo una **Arquitectura Hexagonal**, una base de datos **PostgreSQL** y servicios de **Firebase** para la autenticación de usuarios y el almacenamiento de imágenes.

La solución seguirá una arquitectura desacoplada en la cual el frontend se comunicará con el backend mediante APIs REST. El backend será responsable de implementar la lógica de negocio purificada de infraestructura, validar las operaciones, gestionar el acceso a los datos y comunicarse con los servicios externos correspondientes.

El desarrollo será incremental y estará acompañado por pruebas automatizadas para validar cada funcionalidad y reducir la posibilidad de introducir errores durante la evolución del sistema.

---

## 2. Definición del Stack Tecnológico

### 2.1 Frontend
* **Tecnologías:**
  * Next.js
  * TypeScript
  * React
  * Tailwind CSS (o librería visual a definir)
* **Responsabilidades:**
  * Consumo de API REST del backend.
  * Firebase Auth SDK para la gestión de login del lado del cliente.
  * Manejo de sesión mediante **Firebase ID Token** enviado en cada solicitud HTTP al backend (Header Authorization Bearer).
  * Validaciones de interfaz de usuario y renderizado responsivo.

### 2.2 Backend
* **Tecnologías:**
  * C# en .NET 8 o superior (ASP.NET Core Web API).
  * Entity Framework Core & Npgsql (Provider para PostgreSQL).
  * Firebase Admin SDK (para validación de tokens en backend).
  * Firebase Storage SDK (o integración equivalente Google Cloud Storage).
  * Swagger / OpenAPI para documentación dinámica de endpoints.
  * FluentValidation para la capa de validación de datos de entrada/DTOs.
  * Serilog para logging estructurado.
  * Librería para generación de reportes en Excel (ClosedXML o EPPlus).

### 2.3 Base de Datos
* **Tecnología:** PostgreSQL.
* **Mapeo y Persistencia:** Entity Framework Core.
* **Estrategia de Identificadores:** `int` único, autoincremental mediante PostgreSQL *Identity Columns*.

### 2.4 Autenticación
* **Tecnología:** Firebase Auth.
* **Responsabilidades:**
  * Login.
  * Recuperación de contraseña.
  * Gestión de identidad externa.
  * Emisión de Firebase ID Token.
  * Eventual verificación de email, si se decide exigirla.

> **Regla de Negocio:** Firebase Auth no debe reemplazar el modelo de usuarios interno del sistema. Su única función es la de un proveedor de identidad (IDP).

### 2.5 Autorización Interna
La autorización real del sistema se manejará en **PostgreSQL** y **ASP.NET Core**. Firebase identifica al usuario, pero el backend decide qué puede hacer.

* **Flujo conceptual de Autorización:**
```text
Firebase UID → Usuario interno → Roles → Permisos → Autorización