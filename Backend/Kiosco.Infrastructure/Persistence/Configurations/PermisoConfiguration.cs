using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Descripcion).HasMaxLength(200);

        builder.HasData(
            new Permiso { Id = 1, Codigo = "usuarios.gestionar", Nombre = "Gestionar Usuarios", Descripcion = "Crear, editar y deshabilitar usuarios", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 2, Codigo = "roles.gestionar", Nombre = "Gestionar Roles", Descripcion = "Asignar roles y permisos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 3, Codigo = "productos.crear", Nombre = "Crear Productos", Descripcion = "Registrar nuevos productos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 4, Codigo = "productos.editar", Nombre = "Editar Productos", Descripcion = "Modificar información de productos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 5, Codigo = "productos.eliminar", Nombre = "Eliminar Productos", Descripcion = "Deshabilitar productos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 6, Codigo = "productos.consultar", Nombre = "Consultar Productos", Descripcion = "Ver catálogo de productos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 7, Codigo = "ventas.crear", Nombre = "Crear Ventas", Descripcion = "Registrar nuevas ventas", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 8, Codigo = "ventas.consultar", Nombre = "Consultar Ventas", Descripcion = "Ver historial de ventas", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 9, Codigo = "ventas.anular", Nombre = "Anular Ventas", Descripcion = "Anular ventas registradas", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 10, Codigo = "caja.abrir", Nombre = "Abrir Caja", Descripcion = "Registrar apertura de caja", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 11, Codigo = "caja.cerrar", Nombre = "Cerrar Caja", Descripcion = "Registrar cierre de caja", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 12, Codigo = "caja.movimientos", Nombre = "Movimientos de Caja", Descripcion = "Registrar ingresos y egresos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 13, Codigo = "informes.consultar", Nombre = "Consultar Informes", Descripcion = "Ver reportes e informes", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 14, Codigo = "informes.exportar", Nombre = "Exportar Informes", Descripcion = "Exportar reportes a Excel", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 15, Codigo = "stock.gestionar", Nombre = "Gestionar Stock", Descripcion = "Administrar inventario", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 16, Codigo = "proveedores.gestionar", Nombre = "Gestionar Proveedores", Descripcion = "Administrar proveedores", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 17, Codigo = "clientes.gestionar", Nombre = "Gestionar Clientes", Descripcion = "Administrar clientes", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permiso { Id = 18, Codigo = "rubros.gestionar", Nombre = "Gestionar Rubros", Descripcion = "Crear y editar rubros", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
