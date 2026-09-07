using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Descripcion).HasMaxLength(200);

        builder.HasData(
            new Rol { Id = 1, Nombre = "Administrador", Descripcion = "Acceso total al sistema", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Rol { Id = 2, Nombre = "Operador Administrativo", Descripcion = "Gestión de operaciones y reportes", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Rol { Id = 3, Nombre = "Empleado", Descripcion = "Operaciones básicas de venta", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
