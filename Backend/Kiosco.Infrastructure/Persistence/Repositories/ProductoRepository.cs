using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
{
    public ProductoRepository(KioscoDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _context.Productos
            .Include(p => p.Rubro)
            .Include(p => p.Stock)
            .Where(p => p.Activo)
            .ToListAsync();
    }

    public override async Task<Producto?> GetByIdAsync(int id)
    {
        return await _context.Productos
            .Include(p => p.Rubro)
            .Include(p => p.Stock)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Producto>> GetByRubroAsync(int rubroId)
    {
        return await _context.Productos
            .Include(p => p.Rubro)
            .Include(p => p.Stock)
            .Where(p => p.RubroId == rubroId && p.Activo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Producto>> SearchAsync(
        string? nombre,
        string? codigo,
        int? rubroId,
        bool? activo,
        bool? conStock)
    {
        var query = _context.Productos
            .Include(p => p.Rubro)
            .Include(p => p.Stock)
            .AsQueryable();

        // Estado (RF-016): por defecto solo activos; permite listar dados de baja si se pide.
        if (activo.HasValue)
            query = query.Where(p => p.Activo == activo.Value);
        else
            query = query.Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(p => p.Nombre.Contains(nombre));

        if (!string.IsNullOrWhiteSpace(codigo))
            query = query.Where(p => p.Codigo.Contains(codigo));

        if (rubroId.HasValue)
            query = query.Where(p => p.RubroId == rubroId.Value);

        // Disponibilidad de stock (RF-016).
        if (conStock == true)
            query = query.Where(p => p.Stock != null && p.Stock.Cantidad > 0);
        else if (conStock == false)
            query = query.Where(p => p.Stock == null || p.Stock.Cantidad <= 0);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Producto>> GetStockBajoAsync()
    {
        return await _context.Productos
            .Include(p => p.Rubro)
            .Include(p => p.Stock)
            .Where(p => p.Activo && p.Stock != null && p.Stock.Cantidad <= p.StockMinimo)
            .ToListAsync();
    }

    // Sin filtro de estado: el índice único de Codigo aplica a todos los registros.
    public async Task<Producto?> GetByCodigoAsync(string codigo)
    {
        return await _context.Productos
            .Include(p => p.Rubro)
            .Include(p => p.Stock)
            .FirstOrDefaultAsync(p => p.Codigo == codigo);
    }
}
