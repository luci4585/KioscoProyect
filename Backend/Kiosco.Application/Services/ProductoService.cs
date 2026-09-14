using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _productoRepository;
    private readonly IRubroRepository _rubroRepository;
    private readonly IStockRepository _stockRepository;

    public ProductoService(
        IProductoRepository productoRepository,
        IRubroRepository rubroRepository,
        IStockRepository stockRepository)
    {
        _productoRepository = productoRepository;
        _rubroRepository = rubroRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<ProductoDto>> GetAllAsync()
    {
        var productos = await _productoRepository.GetAllAsync();
        return productos.Select(p => p.ToDto());
    }

    public async Task<ProductoDto?> GetByIdAsync(int id)
    {
        var producto = await _productoRepository.GetByIdAsync(id);
        return producto?.ToDto();
    }

    public async Task<IEnumerable<ProductoDto>> SearchAsync(string? nombre, string? codigo, int? rubroId)
    {
        var productos = await _productoRepository.SearchAsync(nombre, codigo, rubroId);
        return productos.Select(p => p.ToDto());
    }

    public async Task<ProductoDto> CreateAsync(ProductoCreateDto dto)
    {
        if (!await _rubroRepository.ExistsAsync(dto.RubroId))
            throw new EntityNotFoundException(nameof(Rubro), dto.RubroId);

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Codigo = dto.Codigo,
            RubroId = dto.RubroId,
            PrecioVenta = dto.PrecioVenta,
            Costo = dto.Costo,
            StockMinimo = dto.StockMinimo
        };

        var created = await _productoRepository.CreateAsync(producto);

        var stock = new Stock
        {
            ProductoId = created.Id,
            Cantidad = 0
        };
        await _stockRepository.CreateAsync(stock);

        return created.ToDto();
    }

    public async Task UpdateAsync(int id, ProductoUpdateDto dto)
    {
        var producto = await _productoRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Producto), id);

        if (!await _rubroRepository.ExistsAsync(dto.RubroId))
            throw new EntityNotFoundException(nameof(Rubro), dto.RubroId);

        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.Codigo = dto.Codigo;
        producto.RubroId = dto.RubroId;
        producto.PrecioVenta = dto.PrecioVenta;
        producto.Costo = dto.Costo;
        producto.StockMinimo = dto.StockMinimo;

        await _productoRepository.UpdateAsync(producto);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _productoRepository.ExistsAsync(id))
            throw new EntityNotFoundException(nameof(Producto), id);

        await _productoRepository.DeleteAsync(id);
    }
}
