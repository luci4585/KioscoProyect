using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Descripcion).HasMaxLength(500);
        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(p => p.PrecioVenta).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Costo).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ImagenUrl).HasMaxLength(500);

        builder.HasIndex(p => p.Codigo).IsUnique();

        builder.HasOne(p => p.Rubro)
            .WithMany(r => r.Productos)
            .HasForeignKey(p => p.RubroId);
    }
}
