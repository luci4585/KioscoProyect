using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Cantidad).IsRequired();

        builder.HasOne(s => s.Producto)
            .WithOne(p => p.Stock)
            .HasForeignKey<Stock>(s => s.ProductoId);
    }
}
