using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class RubroConfiguration : IEntityTypeConfiguration<Rubro>
{
    public void Configure(EntityTypeBuilder<Rubro> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Descripcion).HasMaxLength(200);

        builder.HasData(
            new Rubro { Id = 1, Nombre = "Golosinas", Descripcion = "Bombones, chocolates, caramelos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Rubro { Id = 2, Nombre = "Bebidas", Descripcion = "Gaseosas, jugos, aguas, energizantes", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Rubro { Id = 3, Nombre = "Snacks", Descripcion = "Papas fritas, maní, palitos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Rubro { Id = 4, Nombre = "Almacén", Descripcion = "Aceites, harinas, conservas", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Rubro { Id = 5, Nombre = "Lácteos", Descripcion = "Leche, yogur, quesos", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Rubro { Id = 6, Nombre = "Limpieza", Descripcion = "Jabón, detergente, lavandina", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Rubro { Id = 7, Nombre = "Otros", Descripcion = "Productos varios", Activo = true, FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
