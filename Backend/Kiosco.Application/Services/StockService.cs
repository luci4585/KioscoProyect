using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _repository;
    private readonly IProductoRepository _productoRepository;

    public StockService(IStockRepository repository, IProductoRepository productoRepository)
    {
        _repository = repository;
        _productoRepository = productoRepository;
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

    public async Task<IEnumerable<StockDto>> GetAlertasAsync()
    {
        var stocks = await _repository.GetAlertasAsync();
        return stocks.Select(s => s.ToDto());
    }

    public async Task<StockDto> IngresarAsync(int productoId, int cantidad)
    {
        if (!await _productoRepository.ExistsAsync(productoId))
            throw new EntityNotFoundException(nameof(Producto), productoId);

        var stock = await _repository.GetByProductoAsync(productoId);
        if (stock == null)
        {
            // Defensa: el producto existe pero no tiene fila de stock (RF-021).
            stock = new Stock { ProductoId = productoId, Cantidad = cantidad };
            await _repository.CreateAsync(stock);
        }
        else
        {
            stock.Cantidad += cantidad;
            await _repository.UpdateAsync(stock);
        }

        var updated = await _repository.GetByProductoAsync(productoId)
            ?? throw new EntityNotFoundException(nameof(Stock), productoId);
        return updated.ToDto();
    }

    public async Task<StockDto> EgresarAsync(int productoId, int cantidad)
    {
        if (!await _productoRepository.ExistsAsync(productoId))
            throw new EntityNotFoundException(nameof(Producto), productoId);

        var stock = await _repository.GetByProductoAsync(productoId);
        if (stock == null || stock.Cantidad < cantidad)
            throw new BusinessRuleException(
                $"Stock insuficiente para '{stock?.Producto?.Nombre ?? $"producto {productoId}"}'. " +
                $"Disponible: {stock?.Cantidad ?? 0}; solicitado: {cantidad}.");

        stock.Cantidad -= cantidad;
        await _repository.UpdateAsync(stock);

        var updated = await _repository.GetByProductoAsync(productoId)
            ?? throw new EntityNotFoundException(nameof(Stock), productoId);
        return updated.ToDto();
    }
}
