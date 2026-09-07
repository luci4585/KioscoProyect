using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.Property(u => u.FirebaseUid).IsRequired().HasMaxLength(128);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.NombreUsuario).IsUnique();
        builder.HasIndex(u => u.FirebaseUid).IsUnique();

        builder.HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId);

        // Usuario admin de ejemplo (Firebase UID placeholder)
        builder.HasData(
            new Usuario
            {
                Id = 1,
                Nombre = "Admin",
                Apellido = "Sistema",
                NombreUsuario = "admin",
                Email = "admin@kiosco.com",
                FirebaseUid = "ADMIN_FIREBASE_UID_CAMBIAR",
                RolId = 1,
                Activo = true,
                FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
