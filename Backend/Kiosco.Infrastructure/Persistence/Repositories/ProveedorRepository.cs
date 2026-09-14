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
}
