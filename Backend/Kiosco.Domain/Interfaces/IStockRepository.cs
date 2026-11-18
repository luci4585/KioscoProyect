using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IStockRepository : IGenericRepository<Stock>
{
    Task<Stock?> GetByProductoAsync(int productoId);
    Task<bool> HayStockSuficienteAsync(int productoId, int cantidad);

    /// <summary>Stocks con cantidad igual o inferior al mínimo del producto (RF-026).</summary>
    Task<IEnumerable<Stock>> GetAlertasAsync();
}
