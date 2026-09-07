using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IVentaRepository : IGenericRepository<Venta>
{
    Task<IEnumerable<Venta>> GetByFechaAsync(DateTime fecha);
    Task<IEnumerable<Venta>> GetByRangoFechasAsync(DateTime desde, DateTime hasta);
    Task<Venta?> GetWithDetailsAsync(int id);
}
