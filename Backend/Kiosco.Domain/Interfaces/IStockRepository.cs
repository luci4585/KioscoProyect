using Kiosco.Domain.Entities;

namespace Kiosco.Domain.Interfaces;

public interface IStockRepository : IGenericRepository<Stock>
{
    Task<Stock?> GetByProductoAsync(int productoId);
    Task<bool> HayStockSuficienteAsync(int productoId, int cantidad);
}
