using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IStockService
{
    Task<IEnumerable<StockDto>> GetAllAsync();
    Task<StockDto?> GetByProductoAsync(int productoId);
}
