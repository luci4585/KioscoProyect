using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class DetalleIngresoMercaderiaConfiguration : IEntityTypeConfiguration<DetalleIngresoMercaderia>
{
    public void Configure(EntityTypeBuilder<DetalleIngresoMercaderia> builder)
    {
        builder.HasKey(dim => dim.Id);
        builder.Property(dim => dim.CostoUnitario).HasColumnType("decimal(18,2)");

        builder.HasOne(dim => dim.IngresoMercaderia)
            .WithMany(im => im.Detalles)
            .HasForeignKey(dim => dim.IngresoMercaderiaId);

        builder.HasOne(dim => dim.Producto)
            .WithMany(p => p.DetallesIngreso)
            .HasForeignKey(dim => dim.ProductoId);
    }
}
