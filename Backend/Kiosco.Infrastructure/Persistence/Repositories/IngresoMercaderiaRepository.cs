using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class IngresoMercaderiaRepository : GenericRepository<IngresoMercaderia>, IIngresoMercaderiaRepository
{
    public IngresoMercaderiaRepository(KioscoDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<IngresoMercaderia>> GetAllAsync()
    {
        return await _context.IngresosMercaderia
            .Include(im => im.Proveedor)
            .OrderByDescending(im => im.Fecha)
            .ToListAsync();
    }

    public async Task<IngresoMercaderia?> GetWithDetailsAsync(int id)
    {
        return await _context.IngresosMercaderia
            .Include(im => im.Proveedor)
            .Include(im => im.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(im => im.Id == id);
    }

    public async Task<IEnumerable<IngresoMercaderia>> GetByProveedorAsync(int proveedorId)
    {
        return await _context.IngresosMercaderia
            .Include(im => im.Proveedor)
            .Where(im => im.ProveedorId == proveedorId)
            .OrderByDescending(im => im.Fecha)
            .ToListAsync();
    }

    public async Task<IEnumerable<IngresoMercaderia>> GetByRangoFechasAsync(DateTime desde, DateTime hasta)
    {
        return await _context.IngresosMercaderia
            .Include(im => im.Proveedor)
            .Where(im => im.Fecha >= desde && im.Fecha <= hasta)
            .OrderByDescending(im => im.Fecha)
            .ToListAsync();
    }
}
