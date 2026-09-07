using Kiosco.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Context;

public class KioscoDbContext : DbContext
{
    public KioscoDbContext(DbContextOptions<KioscoDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Rubro> Rubros => Set<Rubro>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
    public DbSet<IngresoMercaderia> IngresosMercaderia => Set<IngresoMercaderia>();
    public DbSet<DetalleIngresoMercaderia> DetallesIngresoMercaderia => Set<DetalleIngresoMercaderia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KioscoDbContext).Assembly);
    }
}
