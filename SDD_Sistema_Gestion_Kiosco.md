# SDD — Sistema de Gestión de Kiosco

## Índice

1. Requerimientos funcionales
2. Especificaciones técnicas y requerimientos no funcionales
3. Arquitectura general
4. Arquitectura hexagonal del backend
5. Base técnica
6. Estrategia de testing
7. Roadmap de implementación
8. Criterios de finalización

---

# PARTE I — REQUERIMIENTOS FUNCIONALES

## 1. Objetivo

Definir las funcionalidades que deberá proporcionar el Sistema de Gestión de Kiosco para permitir la administración integral del negocio, incluyendo la gestión de usuarios y autenticación, productos, rubros, stock, clientes, proveedores, ingreso de mercadería, ventas, caja e informes.

El sistema deberá permitir registrar, consultar, modificar y controlar la información de las distintas áreas, automatizar procesos como la actualización de stock y el cálculo de ventas, ingresos, egresos y ganancias, y generar informes diarios e históricos que permitan conocer el estado económico del negocio, incluyendo el monto inicial y final de caja, las ventas realizadas, los costos y las ganancias obtenidas.

Asimismo, deberá mantener la información organizada, consistente y segura mediante el control de acceso según los roles y permisos de los usuarios.

---

## 2. Gestión de usuarios y autenticación

### RF-001 — Inicio de sesión

El sistema deberá permitir que los usuarios registrados inicien sesión mediante sus credenciales.

El usuario deberá proporcionar, como mínimo:

- Correo electrónico o identificador de acceso.
- Contraseña.

La autenticación será gestionada mediante Firebase Authentication.

### RF-002 — Cierre de sesión

El sistema deberá permitir al usuario cerrar su sesión de manera segura.

### RF-003 — Registro de usuarios

El sistema deberá permitir registrar nuevos usuarios correspondientes a los trabajadores del kiosco.

Para cada usuario se deberán almacenar datos básicos como:

- Nombre.
- Apellido.
- Nombre de usuario.
- Correo electrónico.
- Rol.
- Estado del usuario.
- Firebase UID.

Las credenciales de autenticación deberán ser gestionadas mediante Firebase Authentication.

### RF-004 — Modificación de usuarios

El sistema deberá permitir modificar los datos de los usuarios registrados.

### RF-005 — Baja de usuarios

El sistema deberá permitir deshabilitar usuarios sin eliminar físicamente su información de la base de datos.

### RF-006 — Roles de usuario

El sistema deberá permitir asignar diferentes roles a los usuarios.

Como mínimo se contemplarán:

- Administrador general.
- Operador administrativo.
- Empleado.

### RF-007 — Control de acceso

El sistema deberá restringir el acceso a las funcionalidades según los permisos correspondientes al usuario autenticado.

### RF-008 — Permisos granulares

La arquitectura deberá permitir asociar permisos específicos a los roles para evitar depender exclusivamente de roles rígidos.

---

## 3. Gestión de rubros

### RF-009 — Registro de rubros

El sistema deberá permitir registrar rubros para clasificar los productos del kiosco.

Ejemplos:

- Golosinas.
- Bebidas.
- Snacks.
- Almacén.
- Lácteos.
- Limpieza.
- Otros.

### RF-010 — Consulta de rubros

El sistema deberá permitir consultar y visualizar los rubros registrados.

### RF-011 — Modificación de rubros

El sistema deberá permitir modificar la información de un rubro.

### RF-012 — Baja de rubros

El sistema deberá permitir deshabilitar rubros que ya no se utilicen, manteniendo la información histórica asociada.

---

## 4. Gestión de productos

### RF-013 — Registro de productos

El sistema deberá permitir registrar los productos comercializados por el kiosco.

Cada producto deberá poder contener, como mínimo:

- Nombre.
- Descripción.
- Código identificador.
- Rubro.
- Precio de venta.
- Costo.
- Stock disponible.
- Stock mínimo.
- Estado.
- Imagen.

### RF-014 — Identificación por rubro

Cada producto deberá estar asociado a un rubro que permita identificar y clasificar el tipo de producto.

### RF-015 — Consulta de productos

El sistema deberá permitir consultar y visualizar los productos registrados.

### RF-016 — Búsqueda y filtrado de productos

El sistema deberá permitir buscar y filtrar productos mediante criterios como:

- Nombre.
- Código.
- Rubro.
- Estado.
- Disponibilidad de stock.

### RF-017 — Modificación de productos

El sistema deberá permitir modificar la información de los productos registrados.

### RF-018 — Baja de productos

El sistema deberá permitir deshabilitar productos que ya no sean comercializados, manteniendo su información histórica.

### RF-019 — Gestión de precios

El sistema deberá permitir registrar y actualizar el precio de venta y costo de cada producto.

### RF-020 — Gestión de imágenes

El sistema deberá permitir asociar imágenes a los productos.

Las imágenes deberán almacenarse mediante Firebase Storage.

---

## 5. Gestión de stock

### RF-021 — Consulta de stock

El sistema deberá permitir consultar la cantidad disponible de cada producto.

### RF-022 — Ingreso de stock

El sistema deberá permitir registrar ingresos de mercadería y actualizar las cantidades disponibles.

### RF-023 — Egreso de stock

El sistema deberá permitir registrar egresos de productos y actualizar las cantidades disponibles.

### RF-024 — Actualización automática del stock por ventas

Al confirmar una venta, el sistema deberá disminuir automáticamente del stock la cantidad correspondiente de cada producto vendido.

### RF-025 — Control de stock insuficiente

El sistema deberá impedir la confirmación de una venta cuando la cantidad solicitada de un producto supere el stock disponible.

### RF-026 — Identificación de stock bajo

El sistema deberá permitir identificar productos cuya cantidad disponible sea igual o inferior al stock mínimo establecido.

---

## 6. Gestión de clientes

### RF-027 — Registro de clientes

El sistema deberá permitir registrar clientes.

Como mínimo:

- Nombre.
- Apellido.
- Documento.
- Teléfono.
- Correo electrónico.
- Dirección.
- Estado.

### RF-028 — Consulta de clientes

El sistema deberá permitir consultar y visualizar los clientes registrados.

### RF-029 — Búsqueda de clientes

El sistema deberá permitir buscar clientes mediante diferentes criterios.

### RF-030 — Modificación de clientes

El sistema deberá permitir modificar los datos de un cliente.

### RF-031 — Baja de clientes

El sistema deberá permitir deshabilitar clientes sin eliminar físicamente su información.

---

## 7. Gestión de proveedores

### RF-032 — Registro de proveedores

El sistema deberá permitir registrar proveedores de mercadería.

Como mínimo:

- Razón social o nombre.
- CUIT.
- Teléfono.
- Correo electrónico.
- Dirección.
- Estado.

### RF-033 — Consulta de proveedores

El sistema deberá permitir consultar los proveedores registrados.

### RF-034 — Modificación de proveedores

El sistema deberá permitir modificar los datos de un proveedor.

### RF-035 — Baja de proveedores

El sistema deberá permitir deshabilitar proveedores que ya no trabajen con el kiosco.

---

## 8. Gestión de mercadería

### RF-036 — Registro de ingreso de mercadería

El sistema deberá permitir registrar el ingreso de mercadería proveniente de proveedores.

### RF-037 — Asociación de mercadería con proveedor

El sistema deberá permitir asociar cada ingreso de mercadería con el proveedor correspondiente.

### RF-038 — Actualización de stock por ingreso

Al registrar un ingreso de mercadería, el sistema deberá incrementar automáticamente el stock de los productos correspondientes.

### RF-039 — Registro de costo de mercadería

El sistema deberá permitir registrar el costo de adquisición de los productos ingresados.

---

## 9. Gestión de ventas

### RF-040 — Registro de ventas

El sistema deberá permitir registrar las ventas realizadas en el kiosco.

Cada venta deberá registrar:

- Fecha y hora.
- Usuario que realizó la venta.
- Cliente, cuando corresponda.
- Productos vendidos.
- Cantidades.
- Precio unitario.
- Subtotal.
- Total.
- Método de pago.

### RF-041 — Agregar productos a una venta

El sistema deberá permitir agregar uno o varios productos a una venta.

### RF-042 — Cálculo de subtotales

El sistema deberá calcular automáticamente el subtotal correspondiente a cada producto según cantidad y precio unitario.

### RF-043 — Cálculo del total

El sistema deberá calcular automáticamente el importe total de la venta.

### RF-044 — Selección del método de pago

El sistema deberá permitir seleccionar el método de pago utilizado.

Como mínimo:

- Efectivo.
- Tarjeta de débito.
- Tarjeta de crédito.
- Transferencia.

### RF-045 — Asociación de cliente

El sistema deberá permitir asociar una venta a un cliente registrado.

La asociación podrá ser opcional.

### RF-046 — Registro del usuario vendedor

Cada venta deberá registrar el usuario que realizó la operación.

### RF-047 — Actualización de stock por venta

Al confirmar una venta, el sistema deberá actualizar automáticamente el stock de los productos involucrados.

### RF-048 — Consulta de ventas

El sistema deberá permitir consultar las ventas realizadas.

### RF-049 — Consulta del detalle de una venta

El sistema deberá permitir consultar el detalle de una venta y visualizar productos, cantidades, precios y subtotales.

### RF-050 — Anulación de ventas

El sistema deberá permitir anular una venta registrada conservando su información histórica.

Cuando corresponda, la anulación deberá restituir las cantidades correspondientes al stock y ajustar los movimientos de caja asociados.

---

## 10. Gestión de caja

### RF-051 — Apertura de caja

El sistema deberá permitir registrar la apertura de una caja indicando el monto inicial disponible.

### RF-052 — Registro de movimientos de caja

El sistema deberá permitir registrar movimientos de caja correspondientes a ingresos y egresos.

Cada movimiento deberá registrar:

- Fecha y hora.
- Tipo de movimiento.
- Monto.
- Descripción.
- Usuario responsable.

### RF-053 — Asociación de ventas con caja

El sistema deberá registrar en caja los movimientos monetarios correspondientes a las ventas realizadas.

### RF-054 — Consulta de movimientos de caja

El sistema deberá permitir consultar los movimientos registrados en caja.

### RF-055 — Resumen de caja

El sistema deberá permitir consultar:

- Monto inicial.
- Total de ingresos.
- Total de egresos.
- Saldo resultante.

### RF-056 — Cierre de caja

El sistema deberá permitir registrar el cierre de caja y obtener el saldo correspondiente al período.

### RF-057 — Diferencia de caja

El sistema deberá permitir comparar el monto final calculado por el sistema con el monto final informado físicamente por el usuario, identificando posibles diferencias.

### RF-058 — Historial de cajas

El sistema deberá permitir consultar las aperturas, movimientos y cierres de cajas anteriores.

---

## 11. Informes y reportes

### RF-059 — Reporte de ventas

El sistema deberá permitir consultar las ventas realizadas durante un período determinado.

### RF-060 — Filtrado de ventas

El sistema deberá permitir filtrar las ventas por:

- Fecha.
- Usuario.
- Cliente.
- Método de pago.

### RF-061 — Reporte de stock

El sistema deberá permitir consultar el estado actual del stock y detectar productos con cantidades bajas.

### RF-062 — Reporte de caja

El sistema deberá permitir consultar los movimientos y resultados de caja correspondientes a un período determinado.

### RF-063 — Productos más vendidos

El sistema deberá permitir identificar los productos con mayor cantidad de unidades vendidas durante un período determinado.

### RF-064 — Resumen general del día

El sistema deberá permitir consultar un resumen de las operaciones realizadas durante un día determinado.

Deberá mostrar:

- Fecha.
- Monto inicial de caja.
- Total de ingresos.
- Total de egresos.
- Total de ventas.
- Costo de los productos vendidos.
- Ganancia obtenida.
- Monto final de caja.

### RF-065 — Cálculo de ganancia diaria

El sistema deberá calcular y mostrar la ganancia obtenida durante un día determinado.

El cálculo general será:

```text
Ganancia = Total de ventas − Costo de productos vendidos
```

### RF-066 — Consulta de ganancia por período

El sistema deberá permitir consultar la ganancia correspondiente a un día o rango de fechas.

### RF-067 — Cálculo del monto final de caja

El sistema deberá calcular el monto final disponible en caja.

El cálculo general será:

```text
Monto final = Monto inicial + Ingresos − Egresos
```

### RF-068 — Informe de cierre diario

El sistema deberá generar un informe de cierre diario que incluya:

- Monto inicial.
- Ingresos.
- Egresos.
- Monto final.
- Cantidad de ventas.
- Total vendido.
- Ventas por método de pago.
- Costo de productos vendidos.
- Ganancia del día.
- Usuario responsable de la apertura.
- Usuario responsable del cierre.
- Fecha de la jornada.

### RF-069 — Historial de cierres de caja

El sistema deberá permitir consultar cierres de caja anteriores.

### RF-070 — Consulta de informes históricos

El sistema deberá permitir consultar informes de días anteriores mediante una fecha o rango de fechas.

### RF-071 — Indicadores del día

El sistema deberá mostrar indicadores correspondientes a la jornada actual, incluyendo:

- Total vendido.
- Ganancia acumulada.
- Dinero disponible en caja.
- Cantidad de ventas.
- Productos vendidos.

### RF-072 — Exportación de informes

El sistema deberá permitir exportar determinados informes y listados a formato Excel.

Como mínimo, se deberá prever la posibilidad de exportar:

- Ventas.
- Stock.
- Movimientos de caja.
- Ingresos de mercadería.
- Informes diarios.
- Ganancias por período.

---

## 12. Eliminación lógica e información histórica

### RF-073 — Baja lógica

El sistema deberá utilizar baja lógica para las entidades cuya eliminación pueda afectar información histórica.

### RF-074 — Conservación de información histórica

El sistema deberá conservar la información histórica relacionada con ventas, movimientos de caja y demás operaciones.

### RF-075 — Restauración de registros

Cuando corresponda, el sistema deberá permitir restaurar registros deshabilitados mediante baja lógica.

---

## 13. Validaciones y consistencia

### RF-076 — Validación de datos

El sistema deberá validar los datos ingresados antes de almacenarlos.

### RF-077 — Prevención de registros inválidos

El sistema deberá impedir operaciones que incumplan las reglas de negocio.

### RF-078 — Identificación de operaciones

Las operaciones relevantes deberán registrar el usuario responsable y la fecha y hora correspondiente.

### RF-079 — Consistencia de información

El sistema deberá mantener la consistencia entre sus diferentes módulos.

Por ejemplo, una venta confirmada deberá reflejarse correctamente en:

- Historial de ventas.
- Stock.
- Caja.
- Informes correspondientes.

---

# PARTE II — ESPECIFICACIONES TÉCNICAS Y REQUERIMIENTOS NO FUNCIONALES

## 14. Objetivo técnico

Definir las tecnologías, arquitectura, prácticas de desarrollo, mecanismos de seguridad, almacenamiento de información y estrategia de pruebas que deberán utilizarse para construir el Sistema de Gestión de Kiosco.

El sistema deberá desarrollarse de manera modular, mantenible, escalable y verificable, utilizando una separación clara entre frontend, backend, servicios externos y base de datos.

El backend deberá utilizar Arquitectura Hexagonal (Ports and Adapters), manteniendo el núcleo de negocio independiente de tecnologías externas.

---

## 15. Backend

### RNF-001 — Lenguaje

El backend deberá desarrollarse utilizando C#.

### RNF-002 — Framework

El backend deberá utilizar ASP.NET Core Web API.

### RNF-003 — Versión de .NET

El proyecto deberá utilizar .NET 8 o superior, priorizando una versión LTS cuando corresponda.

### RNF-004 — Arquitectura

El backend deberá implementarse como una API REST utilizando Arquitectura Hexagonal.

La arquitectura deberá separar:

- Dominio.
- Aplicación.
- Puertos.
- Adaptadores.
- Infraestructura.
- API/presentación.

### RNF-005 — ORM

El acceso a datos deberá realizarse utilizando Entity Framework Core.

### RNF-006 — Proveedor PostgreSQL

La comunicación entre Entity Framework Core y PostgreSQL deberá utilizar Npgsql.

---

## 16. Base de datos

### RNF-007 — Motor de base de datos

El sistema deberá utilizar PostgreSQL como base de datos principal.

### RNF-008 — Integridad de datos

La base de datos deberá utilizar:

- Claves primarias.
- Claves foráneas.
- Restricciones de integridad.
- Relaciones correctamente definidas.
- Tipos de datos adecuados.
- Índices cuando sean necesarios.

### RNF-009 — Identificadores numéricos autoincrementales

Todas las entidades persistidas que requieran identificación única deberán utilizar identificadores numéricos enteros como claves primarias.

Los identificadores deberán:

- Ser de tipo numérico entero.
- Ser únicos dentro de su tabla.
- Generarse automáticamente.
- Ser autoincrementales.
- No ser ingresados manualmente por el usuario.

Ejemplos:

```text
id_producto
id_cliente
id_usuario
id_venta
id_proveedor
id_rubro
id_caja
id_movimiento
```

Las claves foráneas deberán utilizar tipos compatibles con las claves primarias correspondientes.

Entity Framework Core deberá configurarse para reconocer estas claves como valores generados automáticamente por PostgreSQL.

Se deberá utilizar la estrategia de generación mediante Identity Columns de PostgreSQL para los identificadores `int` correspondientes.

No se deberán utilizar identificadores `Guid`, UUID o `string` como claves primarias de las entidades principales salvo que exista una justificación técnica documentada.

---

## 17. Frontend

### RNF-010 — Framework frontend

El frontend deberá desarrollarse utilizando Next.js.

### RNF-011 — Lenguaje frontend

El frontend deberá utilizar TypeScript.

### RNF-012 — Biblioteca de interfaz

El frontend deberá utilizar React.

### RNF-013 — Estilos

Se utilizará Tailwind CSS o una librería visual equivalente que sea definida al inicio de la implementación.

### RNF-014 — Separación frontend/backend

El frontend deberá permanecer desacoplado del backend.

No deberá acceder directamente a PostgreSQL.

Toda comunicación con la información de negocio deberá realizarse mediante las APIs REST del backend.

### RNF-015 — Autenticación frontend

El frontend deberá utilizar Firebase Auth SDK para implementar el login y la gestión de autenticación.

### RNF-016 — Sesión mediante Firebase ID Token

El frontend deberá obtener el Firebase ID Token del usuario autenticado y enviarlo al backend en las solicitudes protegidas mediante:

```text
Authorization: Bearer <Firebase ID Token>
```

---

## 18. APIs

### RNF-017 — APIs REST

El backend deberá exponer sus funcionalidades mediante APIs REST.

Deberán utilizarse apropiadamente:

- GET.
- POST.
- PUT/PATCH.
- DELETE.

### RNF-018 — Nomenclatura de endpoints

Las rutas deberán utilizar nombres consistentes y descriptivos.

Ejemplo:

```text
GET    /api/productos
GET    /api/productos/{id}
POST   /api/productos
PUT    /api/productos/{id}
DELETE /api/productos/{id}
```

### RNF-019 — DTOs

El sistema deberá utilizar DTOs (Data Transfer Objects) para transportar información entre frontend y backend.

No se deberán exponer directamente las entidades internas de Entity Framework cuando no sea necesario.

Se deberán utilizar DTOs adecuados para las diferentes operaciones.

Ejemplo:

```text
ProductoCreateDto
ProductoUpdateDto
ProductoResponseDto
```

### RNF-020 — Códigos HTTP

Las APIs deberán utilizar códigos HTTP adecuados, incluyendo:

- 200 OK.
- 201 Created.
- 204 No Content.
- 400 Bad Request.
- 401 Unauthorized.
- 403 Forbidden.
- 404 Not Found.
- 409 Conflict.
- 500 Internal Server Error.

### RNF-021 — Validación de solicitudes

Las solicitudes deberán validarse antes de ejecutar las operaciones correspondientes.

---

## 19. Autenticación

### RNF-022 — Firebase Authentication

El sistema deberá utilizar Firebase Authentication para la autenticación de usuarios.

### RNF-023 — Firebase Auth SDK

El frontend deberá utilizar Firebase Auth SDK para iniciar y gestionar sesiones.

### RNF-024 — Firebase Admin SDK

El backend deberá utilizar Firebase Admin SDK para validar los tokens emitidos por Firebase.

### RNF-025 — Validación de tokens

El backend deberá validar los Firebase ID Tokens antes de permitir el acceso a endpoints protegidos.

### RNF-026 — Protección de endpoints

Los endpoints que requieran autenticación deberán estar protegidos.

### RNF-027 — Contraseñas

El sistema no deberá almacenar directamente contraseñas de usuarios en PostgreSQL.

La gestión de credenciales deberá delegarse a Firebase Authentication.

### RNF-028 — Recuperación de contraseña

La recuperación de contraseña deberá gestionarse mediante los mecanismos proporcionados por Firebase Authentication.

### RNF-029 — Usuario interno

Firebase Authentication no deberá reemplazar el modelo de usuarios interno del sistema.

PostgreSQL deberá mantener una entidad `Usuario` asociada al Firebase UID para administrar la información necesaria para el negocio.

---

## 20. Autorización interna

### RNF-030 — Autorización mediante backend

La autenticación y la autorización deberán tratarse como responsabilidades diferentes.

Firebase determinará la identidad del usuario y ASP.NET Core determinará qué puede hacer dentro del sistema.

### RNF-031 — Relación de identidad

El flujo conceptual deberá ser:

```text
Firebase UID
    ↓
Usuario interno
    ↓
Roles
    ↓
Permisos
    ↓
Autorización
```

### RNF-032 — Roles iniciales

Se contemplarán inicialmente:

- Administrador general.
- Operador administrativo.
- Empleado.

### RNF-033 — Permisos granulares

La arquitectura deberá prever permisos granulares para evitar quedar limitada a roles rígidos.

Los permisos podrán representar acciones como:

```text
productos.crear
productos.editar
productos.eliminar
ventas.crear
ventas.consultar
caja.abrir
caja.cerrar
informes.consultar
usuarios.gestionar
```

### RNF-034 — Autorización de endpoints

Los endpoints deberán verificar que el usuario autenticado posea el rol o permiso necesario antes de ejecutar operaciones protegidas.

---

## 21. Firebase Storage

### RNF-035 — Almacenamiento de imágenes

Las imágenes de productos deberán almacenarse mediante Firebase Storage.

### RNF-036 — Integración

El backend deberá utilizar Firebase Storage SDK o una integración mediante Google Cloud Storage compatible.

### RNF-037 — Referencias de imágenes

PostgreSQL deberá almacenar únicamente la información necesaria para identificar o acceder a la imagen almacenada en Firebase Storage.

### RNF-038 — Gestión de imágenes

El sistema deberá permitir, según corresponda:

- Cargar imágenes.
- Asociar imágenes a productos.
- Reemplazar imágenes.
- Eliminar imágenes.
- Consultar imágenes.

---

## 22. Arquitectura y organización del código

### RNF-039 — Arquitectura hexagonal

El backend deberá utilizar Arquitectura Hexagonal (Ports and Adapters).

El núcleo de la aplicación deberá permanecer independiente de:

- PostgreSQL.
- Entity Framework Core.
- Npgsql.
- Firebase.
- Serilog.
- ASP.NET Core.
- Servicios externos.

### RNF-040 — Separación de responsabilidades

Los componentes deberán poseer responsabilidades claramente definidas.

Los Controllers no deberán contener lógica de negocio compleja.

### RNF-041 — Dominio

La capa de dominio deberá contener las entidades, reglas de negocio, value objects, enumeraciones y abstracciones propias del negocio.

### RNF-042 — Aplicación

La capa de aplicación deberá contener los casos de uso y la coordinación de las operaciones del sistema.

### RNF-043 — Puertos

Los puertos deberán estar representados mediante interfaces que permitan que el núcleo de la aplicación se comunique con el exterior.

### RNF-044 — Adaptadores

Los adaptadores deberán implementar los puertos y conectar el núcleo con tecnologías externas.

### RNF-045 — Infraestructura

La infraestructura deberá contener las implementaciones concretas relacionadas con:

- PostgreSQL.
- Entity Framework Core.
- Npgsql.
- Firebase.
- Storage.
- Logging.
- Otros servicios externos.

### RNF-046 — Inyección de dependencias

El backend deberá utilizar el sistema de inyección de dependencias proporcionado por ASP.NET Core.

### RNF-047 — Dirección de dependencias

Las dependencias deberán dirigirse hacia el núcleo de la aplicación.

La infraestructura no deberá imponer sus detalles al dominio.

### RNF-048 — Estructura organizada

Como referencia, la estructura del backend será:

```text
Backend/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   ├── Exceptions/
│   └── Interfaces/
│
├── Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   ├── UseCases/
│   ├── Validators/
│   └── Mappings/
│
├── Infrastructure/
│   ├── Persistence/
│   │   ├── Context/
│   │   ├── Configurations/
│   │   ├── Repositories/
│   │   └── Migrations/
│   │
│   ├── Firebase/
│   │   ├── Authentication/
│   │   └── Storage/
│   │
│   └── Logging/
│
├── API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Extensions/
│   └── Program.cs
│
└── Tests/
    ├── UnitTests/
    ├── ControllerTests/
    └── IntegrationTests/
```

La estructura podrá evolucionar durante el desarrollo siempre que se mantengan los principios de la arquitectura hexagonal.

---

## 23. Validaciones

### RNF-049 — FluentValidation

El backend deberá utilizar FluentValidation para implementar las validaciones de entrada.

### RNF-050 — Separación de validaciones

Las validaciones deberán mantenerse separadas de la lógica principal de negocio.

### RNF-051 — Validación frontend/backend

Las validaciones del frontend no deberán reemplazar las validaciones del backend.

El backend deberá validar nuevamente toda información recibida desde clientes externos.

---

## 24. Logging

### RNF-052 — Serilog

El backend deberá utilizar Serilog para logging estructurado.

### RNF-053 — Información registrada

Se podrán registrar:

- Solicitudes HTTP.
- Errores.
- Excepciones.
- Advertencias.
- Operaciones importantes.
- Información técnica para diagnóstico.

### RNF-054 — Información sensible

No deberán registrarse:

- Contraseñas.
- Tokens completos.
- Claves privadas.
- Credenciales.
- Información sensible innecesaria.

### RNF-055 — Independencia del logging

El núcleo del sistema no deberá depender directamente de Serilog.

La configuración concreta de Serilog deberá permanecer en infraestructura.

---

## 25. Documentación de API

### RNF-056 — Swagger/OpenAPI

El backend deberá utilizar Swagger/OpenAPI para documentar las APIs.

La documentación deberá incluir:

- Endpoints.
- Métodos HTTP.
- Parámetros.
- DTOs.
- Respuestas.
- Códigos HTTP.
- Requisitos de autenticación.

---

## 26. Exportación de Excel

### RNF-057 — Librería Excel

El sistema deberá utilizar una librería compatible con .NET para generar archivos Excel.

Las opciones iniciales serán:

- ClosedXML.
- EPPlus.

La biblioteca definitiva deberá seleccionarse al implementar el módulo de informes y quedar documentada.

### RNF-058 — Generación en backend

La generación de archivos Excel deberá realizarse en el backend.

### RNF-059 — Informes exportables

Se deberá prever la exportación de:

- Ventas.
- Productos.
- Stock.
- Movimientos de caja.
- Ingresos de mercadería.
- Informes diarios.
- Ganancias por período.

---

## 27. Testing

### RNF-060 — Desarrollo guiado por pruebas

El desarrollo deberá evolucionar acompañado por una batería de pruebas que permita validar cada cambio significativo.

### RNF-061 — Pruebas unitarias

Se deberán implementar pruebas unitarias para validar:

- Reglas de negocio.
- Servicios.
- Casos de uso.
- Validadores.
- Operaciones individuales.

### RNF-062 — Pruebas de Controllers

Se deberán implementar pruebas de Controllers utilizando una base de datos en memoria o proveedor de pruebas equivalente.

Estas pruebas deberán verificar el comportamiento de los endpoints sin afectar la base de datos real.

### RNF-063 — Pruebas de integración

Se deberán implementar pruebas de integración que utilicen una base de datos PostgreSQL destinada exclusivamente a testing.

Estas pruebas deberán validar la interacción real entre:

```text
API
↓
Application
↓
Infrastructure
↓
Entity Framework Core
↓
Npgsql
↓
PostgreSQL
```

### RNF-064 — Pruebas de regresión

Cada modificación importante deberá ejecutar la batería de pruebas existente para verificar que las funcionalidades anteriores continúen funcionando.

### RNF-065 — Validación antes de avanzar

Una funcionalidad no deberá considerarse finalizada mientras existan pruebas fallidas relacionadas con el cambio realizado.

---

## 28. Buenas prácticas

### RNF-066 — Código limpio

El código deberá ser legible, mantenible y estructurado.

### RNF-067 — Evitar duplicación

Se deberá evitar la duplicación innecesaria de código.

### RNF-068 — Nombres descriptivos

Las clases, métodos, propiedades y variables deberán utilizar nombres claros y descriptivos.

### RNF-069 — Responsabilidad única

Las clases y métodos deberán poseer responsabilidades concretas y claramente definidas.

### RNF-070 — Manejo de errores

El backend deberá implementar un manejo centralizado y consistente de errores y excepciones.

### RNF-071 — Variables de entorno

Las claves, credenciales, connection strings y configuraciones sensibles no deberán quedar expuestas directamente en el código fuente.

### RNF-072 — Control de versiones

El proyecto deberá utilizar Git para control de versiones y GitHub como repositorio remoto.

---

# PARTE III — ARQUITECTURA GENERAL

## 29. Arquitectura general

La aplicación seguirá una arquitectura cliente-servidor desacoplada.

El frontend será responsable de la interacción con el usuario, mientras que el backend concentrará las reglas de negocio, validaciones, autorización, acceso a datos e integración con servicios externos.

La comunicación entre frontend y backend se realizará mediante APIs REST utilizando HTTP/HTTPS y datos en formato JSON.

---

## 29.1 Diagrama lógico

```text
                    Usuario navegador
                           │
                           ▼
                    ┌─────────────┐
                    │   Next.js   │
                    │    React    │
                    │ TypeScript  │
                    └──────┬──────┘
                           │
                           │ HTTP/HTTPS
                           │ REST + JSON
                           │
                           │ Firebase ID Token
                           ▼
              ┌──────────────────────────┐
              │    ASP.NET Core Web API  │
              │          C#              │
              └────────────┬─────────────┘
                           │
                           ▼
              ┌──────────────────────────┐
              │ Servicios de aplicación  │
              │        / dominio         │
              └────────────┬─────────────┘
                           │
                           ▼
                    ┌─────────────┐
                    │ PostgreSQL  │
                    └─────────────┘


              ASP.NET Core Web API
                       │
                       ▼
              Firebase Admin SDK
                       │
                       ▼
                Firebase Auth


              ASP.NET Core Web API
                       │
                       ▼
                Firebase Storage
```

---

## 29.2 Flujo general de una solicitud

```text
Usuario
   ↓
Interfaz Next.js
   ↓
Solicitud HTTP
   ↓
Firebase ID Token
   ↓
ASP.NET Core API
   ↓
Autenticación
   ↓
Autorización
   ↓
Controller
   ↓
DTO
   ↓
Caso de uso / Servicio de aplicación
   ↓
Puerto de salida
   ↓
Adaptador
   ↓
PostgreSQL / Firebase
   ↓
Respuesta
   ↓
Next.js
   ↓
Usuario
```

---

# PARTE IV — ARQUITECTURA HEXAGONAL DEL BACKEND

## 30. Arquitectura hexagonal

El backend utilizará Arquitectura Hexagonal, también conocida como Ports and Adapters.

El objetivo es mantener el núcleo del sistema independiente de tecnologías externas.

La lógica de negocio deberá encontrarse en el centro de la aplicación, mientras que las tecnologías externas se conectarán mediante puertos y adaptadores.

---

## 30.1 Componentes

```text
                         ┌──────────────────────┐
                         │      FRONTEND        │
                         │ Next.js + React      │
                         │     TypeScript       │
                         └──────────┬───────────┘
                                    │
                               HTTP / REST
                                    │
                                    ▼
                    ┌─────────────────────────────┐
                    │    ADAPTADORES DE ENTRADA   │
                    │                             │
                    │ ASP.NET Core Controllers    │
                    │ DTOs                        │
                    │ Validaciones                │
                    └──────────────┬──────────────┘
                                   │
                                   ▼
                    ┌─────────────────────────────┐
                    │        PUERTOS DE ENTRADA    │
                    │                             │
                    │ Interfaces de aplicación     │
                    └──────────────┬──────────────┘
                                   │
                                   ▼
              ┌──────────────────────────────────────────┐
              │              NÚCLEO DE LA APP             │
              │                                          │
              │  ┌────────────────────────────────────┐  │
              │  │             DOMINIO                 │  │
              │  │                                    │  │
              │  │ Entidades                          │  │
              │  │ Value Objects                      │  │
              │  │ Reglas de negocio                  │  │
              │  │ Interfaces de dominio              │  │
              │  └────────────────────────────────────┘  │
              │                    │                     │
              │                    ▼                     │
              │  ┌────────────────────────────────────┐  │
              │  │           APLICACIÓN                │  │
              │  │                                    │  │
              │  │ Casos de uso                       │  │
              │  │ Servicios de aplicación            │  │
              │  │ DTOs                               │  │
              │  │ Puertos                             │  │
              │  └────────────────────────────────────┘  │
              │                                          │
              └───────────────────┬──────────────────────┘
                                  │
                           Puertos de salida
                                  │
                  ┌───────────────┼────────────────┐
                  │               │                │
                  ▼               ▼                ▼
        ┌────────────────┐ ┌───────────────┐ ┌───────────────┐
        │   ADAPTADOR    │ │   ADAPTADOR   │ │   ADAPTADOR   │
        │   PostgreSQL   │ │    Firebase   │ │    Logging    │
        │                │ │               │ │               │
        │ EF Core        │ │ Admin SDK     │ │   Serilog     │
        │ Npgsql         │ │ Storage       │ │               │
        └───────┬────────┘ └───────────────┘ └───────────────┘
                │
                ▼
        ┌────────────────┐
        │   PostgreSQL   │
        └────────────────┘
```

---

## 30.2 Dominio

El dominio será el núcleo de la aplicación.

Contendrá:

- Entidades.
- Value Objects.
- Reglas de negocio.
- Enumeraciones.
- Excepciones de dominio.
- Interfaces necesarias para el negocio.

El dominio no deberá depender directamente de PostgreSQL, Firebase, ASP.NET Core, Entity Framework Core o Serilog.

---

## 30.3 Aplicación

La capa de aplicación representará los casos de uso.

Ejemplos:

```text
CrearProducto
ActualizarProducto
RegistrarVenta
ConsultarStock
RegistrarIngresoMercaderia
AbrirCaja
CerrarCaja
RegistrarMovimientoCaja
GenerarInformeDiario
```

La aplicación coordinará los casos de uso y utilizará interfaces para acceder a recursos externos.

---

## 30.4 Puertos

Los puertos serán interfaces mediante las cuales el núcleo se comunicará con el exterior.

### Puertos de entrada

Ejemplos:

```text
IProductoService
IVentaService
IClienteService
IProveedorService
ICajaService
IStockService
IInformeService
```

### Puertos de salida

Ejemplos:

```text
IProductoRepository
IVentaRepository
IStockRepository
IClienteRepository
IProveedorRepository
ICajaRepository
IStorageService
IAuthenticationService
```

---

## 30.5 Adaptadores

Los adaptadores implementarán los puertos y conectarán el sistema con tecnologías externas.

### Adaptadores de entrada

Principalmente:

```text
ASP.NET Core Controllers
```

Responsabilidades:

- Recibir solicitudes HTTP.
- Recibir DTOs.
- Validar entradas.
- Invocar casos de uso.
- Transformar respuestas.
- Devolver códigos HTTP.

### Adaptadores de salida

Principalmente:

```text
PostgreSQL Adapter
Firebase Adapter
Storage Adapter
Logging Adapter
```

---

## 30.6 Persistencia

El acceso a PostgreSQL seguirá el flujo:

```text
IProductoRepository
        ↓
ProductoRepository
        ↓
Entity Framework Core
        ↓
Npgsql
        ↓
PostgreSQL
```

La implementación concreta estará fuera del dominio.

---

## 30.7 Autenticación en arquitectura hexagonal

La autenticación será una integración externa.

El flujo será:

```text
Next.js
   ↓
Firebase Auth SDK
   ↓
Firebase ID Token
   ↓
ASP.NET Core
   ↓
Firebase Admin SDK
   ↓
Validación
   ↓
Firebase UID
   ↓
Usuario interno PostgreSQL
   ↓
Roles / Permisos
   ↓
Autorización
```

---

## 30.8 Storage en arquitectura hexagonal

El almacenamiento de imágenes utilizará un puerto:

```text
IStorageService
       ↓
FirebaseStorageAdapter
       ↓
Firebase Storage
```

El dominio y los casos de uso no deberán depender directamente de Firebase Storage.

---

## 30.9 Logging en arquitectura hexagonal

Serilog será tratado como una dependencia externa.

El núcleo no deberá depender directamente de Serilog.

La infraestructura será responsable de configurar la herramienta de logging.

```text
Application
     ↓
Abstracción de logging
     ↓
Infraestructura
     ↓
Serilog
```

---

## 30.10 Testing y arquitectura hexagonal

La arquitectura deberá permitir reemplazar adaptadores reales por implementaciones de prueba.

Ejemplo:

```text
                 VentaService
                      │
                IStockRepository
                      │
            ┌─────────┴─────────┐
            │                   │
            ▼                   ▼
     PostgreSQL real       Fake Repository
                               │
                               ▼
                        Base de prueba
```

Esto permitirá probar la lógica de negocio sin depender obligatoriamente de PostgreSQL o Firebase.

Las pruebas de integración utilizarán los adaptadores reales.

---

# PARTE V — BASE TÉCNICA

## 31. Tecnologías generales

El stack tecnológico definido para el proyecto será:

| Componente | Tecnología |
|---|---|
| Lenguaje backend | C# |
| Framework backend | ASP.NET Core Web API |
| Runtime | .NET 8 o superior |
| Arquitectura backend | Hexagonal / Ports and Adapters |
| ORM | Entity Framework Core |
| PostgreSQL provider | Npgsql |
| Base de datos | PostgreSQL |
| Frontend | Next.js |
| UI | React |
| Lenguaje frontend | TypeScript |
| Estilos | Tailwind CSS o librería visual a definir |
| Autenticación frontend | Firebase Auth SDK |
| Autenticación backend | Firebase Admin SDK |
| Storage | Firebase Storage / Google Cloud Storage compatible |
| Validaciones | FluentValidation |
| Logging | Serilog |
| Documentación API | Swagger / OpenAPI |
| Excel | ClosedXML o EPPlus |
| Testing | Unitarios, Controllers e integración |
| Control de versiones | Git |
| Repositorio | GitHub |
| Herramienta de desarrollo asistido | OpenCode |

---

## 31.1 Frontend

### Tecnología

- Next.js.
- TypeScript.
- React.
- Tailwind CSS o librería visual a definir.
- Consumo de API REST del backend.
- Firebase Auth SDK para login.
- Manejo de sesión mediante Firebase ID Token enviado al backend.

### Responsabilidades

El frontend será responsable de:

- Interfaces.
- Navegación.
- Formularios.
- Tablas.
- Dashboard.
- Gestión de sesión.
- Visualización de información.
- Consumo de APIs REST.
- Interacción con Firebase Authentication.

El frontend no deberá acceder directamente a PostgreSQL.

---

## 31.2 Backend

### Tecnología

- ASP.NET Core Web API.
- .NET 8 o superior.
- Entity Framework Core.
- Npgsql para PostgreSQL.
- Firebase Admin SDK.
- Firebase Storage SDK o integración mediante Google Cloud Storage compatible.
- Swagger/OpenAPI para documentación.
- FluentValidation para validaciones.
- Serilog para logging estructurado.
- Librería para generación de Excel, por ejemplo ClosedXML o EPPlus.

### Responsabilidades

El backend será responsable de:

- Exponer APIs REST.
- Procesar solicitudes.
- Ejecutar casos de uso.
- Aplicar reglas de negocio.
- Validar datos.
- Autorizar operaciones.
- Persistir información.
- Integrarse con Firebase.
- Generar informes.
- Generar archivos Excel.
- Registrar logs.

---

## 31.3 PostgreSQL

PostgreSQL será la fuente principal de persistencia de la información del negocio.

Entidades principales previstas:

```text
Usuario
Rol
Permiso
Rubro
Producto
Cliente
Proveedor
Stock
Venta
DetalleVenta
Caja
MovimientoCaja
IngresoMercaderia
DetalleIngresoMercaderia
```

Los identificadores principales serán `int` autoincrementales mediante Identity Columns.

---

## 31.4 Firebase Authentication

Firebase Authentication será responsable de:

- Registro.
- Login.
- Logout.
- Recuperación de contraseña.
- Identificación externa.
- Emisión de Firebase ID Token.
- Verificación de identidad.

No reemplazará al modelo de usuarios interno.

---

## 31.5 Firebase Storage

Firebase Storage será utilizado para:

- Imágenes de productos.
- Archivos que eventualmente requiera el sistema.

PostgreSQL almacenará las referencias necesarias, no los archivos binarios.

---

## 31.6 Swagger/OpenAPI

Swagger/OpenAPI permitirá documentar y probar las APIs.

La documentación deberá mantenerse actualizada a medida que evolucionen los endpoints.

---

## 31.7 FluentValidation

FluentValidation permitirá implementar las validaciones de entrada y mantenerlas separadas de los Controllers.

---

## 31.8 Serilog

Serilog proporcionará logging estructurado y permitirá registrar información técnica relevante sin incluir información sensible.

---

## 31.9 Excel

ClosedXML o EPPlus permitirá generar archivos `.xlsx` desde el backend.

La selección definitiva deberá quedar registrada en la documentación técnica.

---

# PARTE VI — ESTRATEGIA DE TESTING

## 32. Objetivo

El desarrollo deberá evolucionar acompañado por pruebas automatizadas.

Cada cambio significativo deberá contar con pruebas que permitan comprobar que la funcionalidad nueva funciona y que las funcionalidades anteriores no se rompieron.

---

## 32.1 Pruebas unitarias

Se utilizarán para:

- Reglas de dominio.
- Casos de uso.
- Servicios.
- Validadores.
- Cálculos.
- Operaciones individuales.

Ejemplos:

```text
CalcularSubtotal
CalcularTotalVenta
CalcularGanancia
CalcularMontoFinalCaja
ValidarStockDisponible
```

---

## 32.2 Pruebas de Controllers

Los Controllers deberán probarse utilizando una base de datos en memoria o proveedor de pruebas equivalente.

Se deberán validar escenarios como:

- Crear producto.
- Consultar producto.
- Actualizar producto.
- Eliminar producto.
- Registrar venta.
- Consultar ventas.
- Abrir caja.
- Cerrar caja.

---

## 32.3 Pruebas de integración

Las pruebas de integración deberán utilizar una base PostgreSQL de testing.

Deberán comprobar la interacción real de:

```text
Controller
    ↓
Application
    ↓
Domain
    ↓
Infrastructure
    ↓
EF Core
    ↓
Npgsql
    ↓
PostgreSQL
```

---

## 32.4 Pruebas de regresión

Después de cada modificación importante se deberá ejecutar la batería completa de pruebas disponible.

---

## 32.5 Flujo de testing

```text
Requerimiento
      ↓
Diseño
      ↓
Implementación
      ↓
Prueba unitaria
      ↓
Prueba de Controller
      ↓
Prueba de integración
      ↓
Corrección
      ↓
Regresión
      ↓
Documentación
```

---

# PARTE VII — ROADMAP DE IMPLEMENTACIÓN

## 33. Objetivo del roadmap

Definir la ruta de implementación del Sistema de Gestión de Kiosco, estableciendo un orden progresivo para el análisis, diseño, desarrollo, pruebas, integración y entrega.

El proyecto se desarrollará de manera incremental.

Cada etapa deberá dejar una base funcional y verificable antes de avanzar hacia funcionalidades que dependan de ella.

---

## 33.1 Etapa 0 — Preparación

### Objetivo

Preparar el entorno y crear la estructura inicial.

### Actividades

- Crear repositorio Git.
- Configurar GitHub.
- Crear solución backend.
- Crear proyecto frontend.
- Configurar .NET.
- Configurar Next.js.
- Configurar TypeScript.
- Configurar PostgreSQL.
- Configurar Firebase.
- Configurar variables de entorno.
- Configurar proyectos de testing.
- Configurar Swagger.
- Definir estructura hexagonal.

### Resultado

Proyecto base ejecutable.

---

## 33.2 Etapa 1 — Diseño de base de datos

### Objetivo

Diseñar el modelo de persistencia.

### Actividades

Definir:

- Usuarios.
- Roles.
- Permisos.
- Rubros.
- Productos.
- Clientes.
- Proveedores.
- Stock.
- Ventas.
- Detalles de venta.
- Cajas.
- Movimientos.
- Ingresos de mercadería.

Configurar:

- Claves primarias.
- Claves foráneas.
- Relaciones.
- Índices.
- Identity Columns.
- Migraciones.

### Resultado

Modelo de datos y primera migración funcional.

---

## 33.3 Etapa 2 — Arquitectura base del backend

### Objetivo

Crear la estructura hexagonal.

### Actividades

Crear:

```text
Domain
Application
Infrastructure
API
Tests
```

Implementar:

- Dependency Injection.
- DbContext.
- Configuración de PostgreSQL.
- Manejo de errores.
- DTOs.
- Mappings.
- Validaciones.
- Logging.

### Resultado

Backend preparado para implementar casos de uso.

---

## 33.4 Etapa 3 — Autenticación y autorización

### Objetivo

Implementar acceso seguro.

### Actividades

- Firebase Authentication.
- Firebase Auth SDK.
- Firebase Admin SDK.
- Login.
- Logout.
- Recuperación de contraseña.
- Firebase ID Token.
- Usuario interno.
- Roles.
- Permisos.
- Protección de endpoints.

### Testing

Validar:

- Usuario autenticado.
- Usuario no autenticado.
- Token inválido.
- Roles.
- Permisos.
- Endpoints protegidos.

---

## 33.5 Etapa 4 — Rubros

### Objetivo

Implementar clasificación de productos.

### Actividades

- Crear rubro.
- Consultar rubros.
- Modificar rubros.
- Baja lógica.
- Tests.
- API.
- Frontend.

---

## 33.6 Etapa 5 — Productos

### Objetivo

Implementar el catálogo.

### Actividades

- Alta.
- Consulta.
- Búsqueda.
- Filtros.
- Modificación.
- Baja lógica.
- Precios.
- Costos.
- Asociación con rubros.
- Imágenes.

### Firebase Storage

Integrar:

- Carga.
- Asociación.
- Reemplazo.
- Eliminación.

### Tests

Validar las operaciones principales y sus relaciones.

---

## 33.7 Etapa 6 — Clientes y proveedores

### Objetivo

Implementar la gestión de terceros.

### Actividades

- CRUD de clientes.
- CRUD de proveedores.
- Búsquedas.
- Baja lógica.
- Tests.

---

## 33.8 Etapa 7 — Stock

### Objetivo

Implementar el control de existencias.

### Actividades

- Consulta.
- Ingreso.
- Egreso.
- Stock mínimo.
- Alertas.
- Actualización automática.

### Tests

Validar:

- Incrementos.
- Disminuciones.
- Stock insuficiente.
- Stock mínimo.

---

## 33.9 Etapa 8 — Ingreso de mercadería

### Objetivo

Registrar mercadería proveniente de proveedores.

### Actividades

- Crear ingreso.
- Asociar proveedor.
- Agregar productos.
- Registrar cantidades.
- Registrar costos.
- Actualizar stock.

### Tests

Verificar que el ingreso actualice correctamente el stock.

---

## 33.10 Etapa 9 — Ventas

### Objetivo

Implementar el proceso principal de comercialización.

### Actividades

- Crear venta.
- Agregar productos.
- Cantidades.
- Subtotales.
- Total.
- Método de pago.
- Cliente.
- Usuario vendedor.
- Descuento automático de stock.
- Consulta.
- Detalle.
- Anulación.

### Tests

Validar:

- Cálculos.
- Stock insuficiente.
- Persistencia.
- Stock.
- Caja.
- Cliente.
- Usuario.
- Anulación.

---

## 33.11 Etapa 10 — Caja

### Objetivo

Implementar el control financiero diario.

### Actividades

- Apertura.
- Monto inicial.
- Ingresos.
- Egresos.
- Movimientos.
- Asociación de ventas.
- Saldo.
- Cierre.
- Diferencias.
- Historial.

### Tests

Validar:

```text
Monto inicial
+
Ingresos
-
Egresos
=
Monto final
```

---

## 33.12 Etapa 11 — Informes y dashboard

### Objetivo

Implementar la visualización del estado del negocio.

### Actividades

- Ventas del día.
- Ganancia.
- Dinero disponible.
- Productos vendidos.
- Productos más vendidos.
- Stock bajo.
- Ingresos.
- Egresos.
- Historial.
- Informes por período.
- Exportación Excel.

### Resultado

Dashboard e informes funcionales.

---

## 33.13 Etapa 12 — Integración general

### Objetivo

Comprobar el funcionamiento conjunto.

### Flujo principal

```text
Proveedor
    ↓
Ingreso de mercadería
    ↓
Actualización de stock
    ↓
Producto disponible
    ↓
Venta
    ↓
Descuento de stock
    ↓
Movimiento de caja
    ↓
Cierre de caja
    ↓
Informe diario
```

---

## 33.14 Etapa 13 — Pruebas de integración completas

### Objetivo

Validar escenarios completos utilizando PostgreSQL de testing.

### Escenario 1 — Producto

```text
Crear rubro
↓
Crear producto
↓
Asignar stock
↓
Consultar producto
```

### Escenario 2 — Mercadería

```text
Crear proveedor
↓
Registrar ingreso
↓
Agregar productos
↓
Actualizar stock
↓
Verificar stock
```

### Escenario 3 — Venta

```text
Abrir caja
↓
Seleccionar cliente
↓
Agregar productos
↓
Confirmar venta
↓
Actualizar stock
↓
Registrar movimiento
↓
Verificar caja
```

### Escenario 4 — Cierre

```text
Ventas
+
Ingresos
-
Egresos
↓
Cierre de caja
↓
Informe diario
```

---

## 33.15 Etapa 14 — Regresión

### Objetivo

Garantizar que los cambios no rompan funcionalidades existentes.

### Actividades

Ejecutar:

- Pruebas unitarias.
- Pruebas de Controllers.
- Pruebas de integración.
- Pruebas de regresión.

---

## 33.16 Etapa 15 — Seguridad y optimización

### Objetivo

Preparar el sistema para su entrega.

### Actividades

Revisar:

- Autenticación.
- Autorización.
- Validaciones.
- Manejo de errores.
- Logs.
- Rendimiento.
- Consultas.
- Variables de entorno.
- Permisos Firebase.
- Seguridad de Storage.
- Código duplicado.
- Organización.

---

## 33.17 Etapa 16 — Documentación

### Objetivo

Documentar el sistema y sus decisiones técnicas.

La documentación deberá incluir:

- SDD.
- Requerimientos funcionales.
- Requerimientos no funcionales.
- Base técnica.
- Arquitectura.
- Modelo de datos.
- API.
- Testing.
- Instalación.
- Configuración.
- Roadmap.
- Decisiones técnicas.

---

## 33.18 Etapa 17 — Preparación para entrega

### Objetivo

Preparar el sistema para la presentación académica.

### Actividades

- Ejecutar todos los tests.
- Verificar funcionalidades principales.
- Corregir errores.
- Revisar documentación.
- Verificar base de datos.
- Verificar configuración.
- Preparar demostración.
- Preparar explicación de arquitectura.
- Preparar explicación del stack tecnológico.
- Preparar explicación de testing.
- Preparar flujos completos de negocio.

---

## 33.19 Flujo general del roadmap

```text
PREPARACIÓN
     ↓
BASE DE DATOS
     ↓
ARQUITECTURA HEXAGONAL
     ↓
AUTENTICACIÓN
     ↓
AUTORIZACIÓN
     ↓
RUBROS
     ↓
PRODUCTOS
     ↓
CLIENTES / PROVEEDORES
     ↓
STOCK
     ↓
INGRESO DE MERCADERÍA
     ↓
VENTAS
     ↓
CAJA
     ↓
INFORMES
     ↓
INTEGRACIÓN
     ↓
TESTING COMPLETO
     ↓
SEGURIDAD / OPTIMIZACIÓN
     ↓
DOCUMENTACIÓN
     ↓
ENTREGA
```

---

## 33.20 Ciclo incremental

El desarrollo no deberá realizarse implementando todo el sistema primero y probándolo al final.

Cada funcionalidad deberá seguir un ciclo:

```text
Requerimiento
      ↓
Diseño
      ↓
Implementación
      ↓
Pruebas
      ↓
Corrección
      ↓
Integración
      ↓
Regresión
      ↓
Documentación
```

Este enfoque permitirá que el sistema evolucione progresivamente y que cada modificación importante quede validada.

---

# PARTE VIII — CRITERIOS DE FINALIZACIÓN

## 34. Criterios de finalización del proyecto

El proyecto se considerará preparado para su entrega cuando:

- Las funcionalidades principales estén implementadas.
- Frontend y backend estén correctamente integrados.
- PostgreSQL almacene correctamente la información.
- Firebase Authentication gestione la autenticación.
- Firebase Storage gestione las imágenes.
- El backend respete la Arquitectura Hexagonal.
- Las APIs REST estén implementadas.
- Los DTOs estén implementados.
- Los identificadores principales sean numéricos autoincrementales.
- FluentValidation esté integrado.
- Serilog esté configurado.
- Swagger/OpenAPI documente las APIs.
- La generación de Excel esté implementada.
- Las pruebas unitarias estén implementadas.
- Las pruebas de Controllers estén implementadas.
- Las pruebas de integración estén implementadas.
- Las pruebas de regresión se ejecuten correctamente.
- No existan errores críticos conocidos.
- La documentación esté actualizada.
- El sistema pueda demostrarse mediante flujos completos de negocio.
- El proyecto pueda ser comprendido y ejecutado siguiendo la documentación.
