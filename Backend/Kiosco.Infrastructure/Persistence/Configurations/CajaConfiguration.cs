using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class CajaConfiguration : IEntityTypeConfiguration<Caja>
{
    public void Configure(EntityTypeBuilder<Caja> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.MontoInicial).HasColumnType("decimal(18,2)");
        builder.Property(c => c.MontoFinal).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Diferencia).HasColumnType("decimal(18,2)");

        builder.HasOne(c => c.UsuarioApertura)
            .WithMany()
            .HasForeignKey(c => c.UsuarioAperturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.UsuarioCierre)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCierreId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
