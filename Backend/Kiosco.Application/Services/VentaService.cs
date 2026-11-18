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
    private readonly IClienteRepository _clienteRepository;
    private readonly ICajaRepository _cajaRepository;
    private readonly IMovimientoCajaRepository _movimientoCajaRepository;

    public VentaService(
        IVentaRepository ventaRepository,
        IProductoRepository productoRepository,
        IStockRepository stockRepository,
        IClienteRepository clienteRepository,
        ICajaRepository cajaRepository,
        IMovimientoCajaRepository movimientoCajaRepository)
    {
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _stockRepository = stockRepository;
        _clienteRepository = clienteRepository;
        _cajaRepository = cajaRepository;
        _movimientoCajaRepository = movimientoCajaRepository;
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
        // RF-045: la asociación de cliente es opcional, pero si se indica debe
        // corresponder a un cliente registrado.
        if (dto.ClienteId.HasValue &&
            !await _clienteRepository.ExistsAsync(dto.ClienteId.Value))
            throw new EntityNotFoundException(nameof(Cliente), dto.ClienteId.Value);

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

        // Devuelve la venta con usuario, cliente y productos resueltos: la
        // respuesta del alta debe tener las mismas descripciones que el detalle.
        var completo = await _ventaRepository.GetWithDetailsAsync(created.Id) ?? created;

        // RF-053: registrar en caja el movimiento monetario de la venta, si hay
        // una caja abierta (si no, la venta queda registrada y el movimiento se
        // hará constar cuando corresponda).
        var cajaAbierta = await _cajaRepository.GetCajaAbiertaAsync();
        if (cajaAbierta != null)
        {
            await _movimientoCajaRepository.CreateAsync(new MovimientoCaja
            {
                CajaId = cajaAbierta.Id,
                Fecha = DateTime.UtcNow,
                TipoMovimiento = "Ingreso",
                Monto = total,
                Descripcion = $"Venta #{created.Id}",
                UsuarioId = usuarioId,
                VentaId = created.Id
            });
        }

        return completo.ToDto();
    }

    public async Task AnularAsync(int id, int usuarioId)
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

        // RF-050/RF-053: ajustar el movimiento de caja asociado mientras su
        // caja siga abierta; un período cerrado no se modifica ("cuando
        // corresponda"). El ajuste es un egreso compensador que conserva el
        // historial (RF-052).
        var movimiento = await _movimientoCajaRepository.GetByVentaIdAsync(id);
        if (movimiento != null)
        {
            var caja = await _cajaRepository.GetByIdAsync(movimiento.CajaId);
            if (caja is { Abierta: true })
            {
                await _movimientoCajaRepository.CreateAsync(new MovimientoCaja
                {
                    CajaId = movimiento.CajaId,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = "Egreso",
                    Monto = movimiento.Monto,
                    Descripcion = $"Anulación venta #{id}",
                    UsuarioId = usuarioId
                });
            }
        }
    }
}
