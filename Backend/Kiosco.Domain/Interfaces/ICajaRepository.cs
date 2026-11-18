using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface ICajaRepository : IGenericRepository<Caja>
{
    Task<Caja?> GetCajaAbiertaAsync();
    Task<Caja?> GetWithMovimientosAsync(int id);
    Task<IEnumerable<Caja>> GetByFechaAsync(DateTime fecha);

    /// <summary>Cajas del período con movimientos y usuarios (RF-062).</summary>
    Task<IEnumerable<Caja>> GetByRangoFechasAsync(DateTime desde, DateTime hasta);
}
