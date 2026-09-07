using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class IngresoMercaderiaConfiguration : IEntityTypeConfiguration<IngresoMercaderia>
{
    public void Configure(EntityTypeBuilder<IngresoMercaderia> builder)
    {
        builder.HasKey(im => im.Id);
        builder.Property(im => im.Observaciones).HasMaxLength(500);

        builder.HasOne(im => im.Proveedor)
            .WithMany(p => p.IngresosMercaderia)
            .HasForeignKey(im => im.ProveedorId);
    }
}
