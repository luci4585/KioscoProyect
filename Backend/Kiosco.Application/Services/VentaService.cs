using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

public class VentaService : IVentaService
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IStockRepository _stockRepository;

    public VentaService(
        IVentaRepository ventaRepository,
        IProductoRepository productoRepository,
        IStockRepository stockRepository)
    {
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<VentaDto>> GetAllAsync()
    {
        var ventas = await _ventaRepository.GetAllAsync();
        return ventas.Select(v => v.ToDto());
    }

    public async Task<VentaDto?> GetByIdAsync(int id)
    {
        var venta = await _ventaRepository.GetWithDetailsAsync(id);
        return venta?.ToDto();
    }

    public async Task<VentaDto> CreateAsync(VentaCreateDto dto, int usuarioId)
    {
        decimal total = 0;
        var detalles = new List<DetalleVenta>();

        foreach (var detalleDto in dto.Detalles)
        {
            var producto = await _productoRepository.GetByIdAsync(detalleDto.ProductoId)
                ?? throw new EntityNotFoundException(nameof(Producto), detalleDto.ProductoId);

            if (!await _stockRepository.HayStockSuficienteAsync(detalleDto.ProductoId, detalleDto.Cantidad))
                throw new BusinessRuleException($"Stock insuficiente para '{producto.Nombre}'. Stock actual: {producto.Stock?.Cantidad ?? 0}");

            var subtotal = producto.PrecioVenta * detalleDto.Cantidad;
            total += subtotal;

            detalles.Add(new DetalleVenta
            {
                ProductoId = detalleDto.ProductoId,
                Cantidad = detalleDto.Cantidad,
                PrecioUnitario = producto.PrecioVenta,
                Subtotal = subtotal
            });
        }

        var venta = new Venta
        {
            Fecha = DateTime.UtcNow,
            UsuarioId = usuarioId,
            ClienteId = dto.ClienteId,
            Total = total,
            MetodoPago = dto.MetodoPago,
            Anulada = false,
            DetallesVenta = detalles
        };

        var created = await _ventaRepository.CreateAsync(venta);

        foreach (var detalle in detalles)
        {
            var stock = await _stockRepository.GetByProductoAsync(detalle.ProductoId);
            if (stock != null)
            {
                stock.Cantidad -= detalle.Cantidad;
                await _stockRepository.UpdateAsync(stock);
            }
        }

        return created.ToDto();
    }

    public async Task AnularAsync(int id)
    {
        var venta = await _ventaRepository.GetWithDetailsAsync(id)
            ?? throw new EntityNotFoundException(nameof(Venta), id);

        if (venta.Anulada)
            throw new BusinessRuleException("La venta ya está anulada.");

        venta.Anulada = true;
        await _ventaRepository.UpdateAsync(venta);

        foreach (var detalle in venta.DetallesVenta)
        {
            var stock = await _stockRepository.GetByProductoAsync(detalle.ProductoId);
            if (stock != null)
            {
                stock.Cantidad += detalle.Cantidad;
                await _stockRepository.UpdateAsync(stock);
            }
        }
    }
}
