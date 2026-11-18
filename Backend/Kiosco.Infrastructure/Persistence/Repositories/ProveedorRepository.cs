using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class ProveedorRepository : GenericRepository<Proveedor>, IProveedorRepository
{
    public ProveedorRepository(KioscoDbContext context) : base(context)
    {
    }

    public async Task<Proveedor?> GetByCuitAsync(string cuit)
    {
        return await _context.Proveedores
            .FirstOrDefaultAsync(p => p.Cuit == cuit && p.Activo);
    }

    public async Task<IEnumerable<Proveedor>> SearchAsync(string? razonSocial, string? cuit)
    {
        var query = _context.Proveedores.Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(razonSocial))
            query = query.Where(p => p.RazonSocial.Contains(razonSocial));

        if (!string.IsNullOrWhiteSpace(cuit))
            query = query.Where(p => p.Cuit != null && p.Cuit.Contains(cuit));

        return await query.ToListAsync();
    }
}
