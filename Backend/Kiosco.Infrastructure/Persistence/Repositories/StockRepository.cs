using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class StockRepository : GenericRepository<Stock>, IStockRepository
{
    public StockRepository(KioscoDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Stock>> GetAllAsync()
    {
        return await _context.Stocks
            .Include(s => s.Producto)
            .Where(s => s.Activo)
            .ToListAsync();
    }

    public async Task<Stock?> GetByProductoAsync(int productoId)
    {
        return await _context.Stocks
            .Include(s => s.Producto)
            .FirstOrDefaultAsync(s => s.ProductoId == productoId);
    }

    public async Task<bool> HayStockSuficienteAsync(int productoId, int cantidad)
    {
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductoId == productoId);

        return stock != null && stock.Cantidad >= cantidad;
    }
}
