using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IVentaRepository : IGenericRepository<Venta>
{
    Task<IEnumerable<Venta>> GetByFechaAsync(DateTime fecha);
    Task<IEnumerable<Venta>> GetByRangoFechasAsync(DateTime desde, DateTime hasta);

    /// <summary>
    /// Ventas del período con detalles y productos (RF-059, RF-063, RF-065):
    /// necesario para costo de lo vendido y rankings.
    /// </summary>
    Task<IEnumerable<Venta>> GetByRangoFechasConDetallesAsync(DateTime desde, DateTime hasta);

    Task<Venta?> GetWithDetailsAsync(int id);
}
