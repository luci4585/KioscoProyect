using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiosco.Infrastructure.Persistence.Configurations;

public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.HasKey(rp => new { rp.RolId, rp.PermisoId });

        builder.HasOne(rp => rp.Rol)
            .WithMany(r => r.RolPermisos)
            .HasForeignKey(rp => rp.RolId);

        builder.HasOne(rp => rp.Permiso)
            .WithMany(p => p.RolPermisos)
            .HasForeignKey(rp => rp.PermisoId);

        // Administrador: todos los permisos
        var adminPermisos = Enumerable.Range(1, 18)
            .Select(p => new RolPermiso { RolId = 1, PermisoId = p })
            .ToList();

        // Operador: permisos operativos
        var operadorPermisos = new List<RolPermiso>
        {
            new() { RolId = 2, PermisoId = 3 },  // productos.crear
            new() { RolId = 2, PermisoId = 4 },  // productos.editar
            new() { RolId = 2, PermisoId = 6 },  // productos.consultar
            new() { RolId = 2, PermisoId = 7 },  // ventas.crear
            new() { RolId = 2, PermisoId = 8 },  // ventas.consultar
            new() { RolId = 2, PermisoId = 9 },  // ventas.anular
            new() { RolId = 2, PermisoId = 10 }, // caja.abrir
            new() { RolId = 2, PermisoId = 11 }, // caja.cerrar
            new() { RolId = 2, PermisoId = 12 }, // caja.movimientos
            new() { RolId = 2, PermisoId = 13 }, // informes.consultar
            new() { RolId = 2, PermisoId = 14 }, // informes.exportar
            new() { RolId = 2, PermisoId = 15 }, // stock.gestionar
            new() { RolId = 2, PermisoId = 16 }, // proveedores.gestionar
            new() { RolId = 2, PermisoId = 17 }, // clientes.gestionar
            new() { RolId = 2, PermisoId = 18 }, // rubros.gestionar
        };

        // Empleado: permisos básicos
        var empleadoPermisos = new List<RolPermiso>
        {
            new() { RolId = 3, PermisoId = 6 },  // productos.consultar
            new() { RolId = 3, PermisoId = 7 },  // ventas.crear
            new() { RolId = 3, PermisoId = 8 },  // ventas.consultar
            new() { RolId = 3, PermisoId = 10 }, // caja.abrir
            new() { RolId = 3, PermisoId = 13 }, // informes.consultar
        };

        builder.HasData(adminPermisos.Concat(operadorPermisos).Concat(empleadoPermisos));
    }
}
