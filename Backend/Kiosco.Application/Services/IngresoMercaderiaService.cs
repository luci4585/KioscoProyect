using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class IngresoMercaderiaService : IIngresoMercaderiaService
{
    private readonly IIngresoMercaderiaRepository _repository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IStockRepository _stockRepository;

    public IngresoMercaderiaService(
        IIngresoMercaderiaRepository repository,
        IProveedorRepository proveedorRepository,
        IProductoRepository productoRepository,
        IStockRepository stockRepository)
    {
        _repository = repository;
        _proveedorRepository = proveedorRepository;
        _productoRepository = productoRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<IngresoMercaderiaDto>> GetAllAsync()
    {
        var ingresos = await _repository.GetAllAsync();
        return ingresos.Select(i => i.ToDto());
    }

    public async Task<IngresoMercaderiaDto?> GetByIdAsync(int id)
    {
        var ingreso = await _repository.GetWithDetailsAsync(id);
        return ingreso?.ToDto();
    }

    public async Task<IngresoMercaderiaDto> CreateAsync(IngresoMercaderiaCreateDto dto)
    {
        if (!await _proveedorRepository.ExistsAsync(dto.ProveedorId))
            throw new EntityNotFoundException(nameof(Proveedor), dto.ProveedorId);

        var detalles = new List<DetalleIngresoMercaderia>();
        foreach (var detalleDto in dto.Detalles)
        {
            if (!await _productoRepository.ExistsAsync(detalleDto.ProductoId))
                throw new EntityNotFoundException(nameof(Producto), detalleDto.ProductoId);

            detalles.Add(new DetalleIngresoMercaderia
            {
                ProductoId = detalleDto.ProductoId,
                Cantidad = detalleDto.Cantidad,
                CostoUnitario = detalleDto.CostoUnitario
            });
        }

        var ingreso = new IngresoMercaderia
        {
            Fecha = DateTime.UtcNow,
            ProveedorId = dto.ProveedorId,
            Observaciones = dto.Observaciones,
            Detalles = detalles
        };

        var created = await _repository.CreateAsync(ingreso);

        foreach (var detalle in detalles)
        {
            var stock = await _stockRepository.GetByProductoAsync(detalle.ProductoId);
            if (stock != null)
            {
                stock.Cantidad += detalle.Cantidad;
                await _stockRepository.UpdateAsync(stock);
            }
            else
            {
                await _stockRepository.CreateAsync(new Stock
                {
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad
                });
            }
        }

        return created.ToDto();
    }
}
