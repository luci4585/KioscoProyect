using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.RazonSocial).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Cuit).HasMaxLength(11);
        builder.Property(p => p.Telefono).HasMaxLength(20);
        builder.Property(p => p.Email).HasMaxLength(150);
        builder.Property(p => p.Direccion).HasMaxLength(200);
    }
}
