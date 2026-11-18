using Kiosco.Domain.Entities;
using Kiosco.Domain.Interfaces;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Infrastructure.Persistence.Repositories;

public class CajaRepository : GenericRepository<Caja>, ICajaRepository
{
    public CajaRepository(KioscoDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Caja>> GetAllAsync()
    {
        // Historial de cajas (RF-058): aperturas y cierres con sus usuarios.
        return await _context.Cajas
            .Include(c => c.UsuarioApertura)
            .Include(c => c.UsuarioCierre)
            .OrderByDescending(c => c.FechaApertura)
            .ToListAsync();
    }

    public async Task<Caja?> GetCajaAbiertaAsync()
    {
        return await _context.Cajas
            .Include(c => c.UsuarioApertura)
            .FirstOrDefaultAsync(c => c.Abierta);
    }

    public async Task<Caja?> GetWithMovimientosAsync(int id)
    {
        return await _context.Cajas
            .Include(c => c.UsuarioApertura)
            .Include(c => c.UsuarioCierre)
            .Include(c => c.Movimientos)
                .ThenInclude(m => m.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Caja>> GetByFechaAsync(DateTime fecha)
    {
        return await _context.Cajas
            .Include(c => c.UsuarioApertura)
            .Include(c => c.UsuarioCierre)
            .Where(c => c.FechaApertura.Date == fecha.Date)
            .OrderByDescending(c => c.FechaApertura)
            .ToListAsync();
    }

    public async Task<IEnumerable<Caja>> GetByRangoFechasAsync(DateTime desde, DateTime hasta)
    {
        return await _context.Cajas
            .Include(c => c.UsuarioApertura)
            .Include(c => c.UsuarioCierre)
            .Include(c => c.Movimientos)
                .ThenInclude(m => m.Usuario)
            .Where(c => c.FechaApertura.Date >= desde.Date && c.FechaApertura.Date <= hasta.Date)
            .OrderByDescending(c => c.FechaApertura)
            .ToListAsync();
    }
}
