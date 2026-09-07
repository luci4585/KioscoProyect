using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.HasKey(dv => dv.Id);
        builder.Property(dv => dv.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Property(dv => dv.Subtotal).HasColumnType("decimal(18,2)");

        builder.HasOne(dv => dv.Venta)
            .WithMany(v => v.DetallesVenta)
            .HasForeignKey(dv => dv.VentaId);

        builder.HasOne(dv => dv.Producto)
            .WithMany(p => p.DetallesVenta)
            .HasForeignKey(dv => dv.ProductoId);
    }
}
