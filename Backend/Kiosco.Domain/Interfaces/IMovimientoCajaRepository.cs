using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IMovimientoCajaRepository : IGenericRepository<MovimientoCaja>
{
    /// <summary>Movimiento asociado a una venta (RF-053 / RF-050).</summary>
    Task<MovimientoCaja?> GetByVentaIdAsync(int ventaId);
}
