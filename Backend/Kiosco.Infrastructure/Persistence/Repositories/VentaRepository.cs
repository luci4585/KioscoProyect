using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class VentaRepository : GenericRepository<Venta>, IVentaRepository
{
    public VentaRepository(KioscoDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Venta>> GetAllAsync()
    {
        return await _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Cliente)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venta>> GetByFechaAsync(DateTime fecha)
    {
        return await _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Cliente)
            .Where(v => v.Fecha.Date == fecha.Date)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venta>> GetByRangoFechasAsync(DateTime desde, DateTime hasta)
    {
        return await _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Cliente)
            .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<Venta?> GetWithDetailsAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
                .ThenInclude(dv => dv.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}
