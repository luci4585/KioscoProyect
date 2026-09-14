using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _repository;

    public StockService(IStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<StockDto>> GetAllAsync()
    {
        var stocks = await _repository.GetAllAsync();
        return stocks.Select(s => s.ToDto());
    }

    public async Task<StockDto?> GetByProductoAsync(int productoId)
    {
        var stock = await _repository.GetByProductoAsync(productoId);
        return stock?.ToDto();
    }
}
