using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface ICajaRepository : IGenericRepository<Caja>
{
    Task<Caja?> GetCajaAbiertaAsync();
    Task<Caja?> GetWithMovimientosAsync(int id);
    Task<IEnumerable<Caja>> GetByFechaAsync(DateTime fecha);
}
