using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class MovimientoCajaRepository : GenericRepository<MovimientoCaja>, IMovimientoCajaRepository
{
    public MovimientoCajaRepository(KioscoDbContext context) : base(context)
    {
    }

    public async Task<MovimientoCaja?> GetByVentaIdAsync(int ventaId)
    {
        return await _context.MovimientosCaja
            .FirstOrDefaultAsync(m => m.VentaId == ventaId);
    }
}
