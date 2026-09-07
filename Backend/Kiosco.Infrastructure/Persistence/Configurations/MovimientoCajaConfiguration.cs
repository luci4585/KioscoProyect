using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
{
    public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
    {
        builder.HasKey(mc => mc.Id);
        builder.Property(mc => mc.TipoMovimiento).IsRequired().HasMaxLength(20);
        builder.Property(mc => mc.Monto).HasColumnType("decimal(18,2)");
        builder.Property(mc => mc.Descripcion).HasMaxLength(200);

        builder.HasOne(mc => mc.Caja)
            .WithMany(c => c.Movimientos)
            .HasForeignKey(mc => mc.CajaId);

        builder.HasOne(mc => mc.Usuario)
            .WithMany(u => u.MovimientosCaja)
            .HasForeignKey(mc => mc.UsuarioId);

        builder.HasOne(mc => mc.Venta)
            .WithOne(v => v.MovimientoCaja)
            .HasForeignKey<MovimientoCaja>(mc => mc.VentaId)
            .IsRequired(false);
    }
}
