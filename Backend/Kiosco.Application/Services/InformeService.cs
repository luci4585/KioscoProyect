using Kiosco.Application.DTOs;
using Kiosco.Application.Interfaces;
using Kiosco.Application.Mappings;
using Kiosco.Domain.Entities;
using Kiosco.Domain.Exceptions;
using Kiosco.Domain.Interfaces;

namespace Kiosco.Application.Services;

/// <summary>
/// Informes y dashboard — Etapa 11 (RoadMap §14; RF-059 a RF-072).
///
/// Nota de costo (RF-065): el esquema no guarda el costo histórico por
/// detalle de venta, por lo que el costo de lo vendido se calcula con el
/// costo vigente del producto (cantidad × costo actual).
/// </summary>
public class InformeService : IInformeService
{
    private readonly IVentaRepository _ventaRepository;
    private readonly ICajaRepository _cajaRepository;
    private readonly IIngresoMercaderiaRepository _ingresoRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IExcelExportService _excel;

    public InformeService(
        IVentaRepository ventaRepository,
        ICajaRepository cajaRepository,
        IIngresoMercaderiaRepository ingresoRepository,
        IStockRepository stockRepository,
        IProductoRepository productoRepository,
        IExcelExportService excel)
    {
        _ventaRepository = ventaRepository;
        _cajaRepository = cajaRepository;
        _ingresoRepository = ingresoRepository;
        _stockRepository = stockRepository;
        _productoRepository = productoRepository;
        _excel = excel;
    }

    private static (DateTime Desde, DateTime HastaExclusiva) Rango(DateTime desde, DateTime hasta)
    {
        if (hasta.Date < desde.Date)
            throw new BusinessRuleException("El período es inválido: la fecha final es anterior a la inicial.");

        // Npgsql exige Kind=Utc para 'timestamp with time zone': las fechas
        // sólo se usan como días, se normalizan a UTC sin convertir la hora.
        var d = DateTime.SpecifyKind(desde.Date, DateTimeKind.Utc);
        return (d, d.AddDays(1));
    }

    // RF-065: Ganancia = Total de ventas − Costo de productos vendidos.
    private static (decimal Total, decimal Costo, decimal Ganancia, int Cantidad, int Unidades)
        CalcularVentas(IEnumerable<Venta> ventas)
    {
        decimal total = 0, costo = 0;
        int cantidad = 0, unidades = 0;

        foreach (var venta in ventas.Where(v => !v.Anulada))
        {
            total += venta.Total;
            cantidad++;
            foreach (var detalle in venta.DetallesVenta ?? Enumerable.Empty<DetalleVenta>())
            {
                costo += detalle.Cantidad * (detalle.Producto?.Costo ?? 0);
                unidades += detalle.Cantidad;
            }
        }

        return (total, costo, total - costo, cantidad, unidades);
    }

    // ── RF-064 / RF-067 / RF-068 / RF-071 ──────────────────────────────────

    public async Task<ResumenDiarioDto> GetResumenDiarioAsync(DateTime fecha)
    {
        // Kind=Utc para los parámetros de Npgsql (ver Rango).
        var desde = DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);
        var hasta = desde.AddDays(1);

        var caja = (await _cajaRepository.GetByFechaAsync(desde)).FirstOrDefault();
        if (caja != null)
            caja = await _cajaRepository.GetWithMovimientosAsync(caja.Id) ?? caja;

        var ingresosCaja = caja?.Movimientos?
            .Where(m => m.Activo && m.TipoMovimiento == "Ingreso").Sum(m => m.Monto) ?? 0;
        var egresosCaja = caja?.Movimientos?
            .Where(m => m.Activo && m.TipoMovimiento == "Egreso").Sum(m => m.Monto) ?? 0;

        var ventas = (await _ventaRepository.GetByRangoFechasConDetallesAsync(desde, hasta))
            .Where(v => v.Fecha >= desde && v.Fecha < hasta)
            .ToList();
        var calc = CalcularVentas(ventas);

        var resumen = new ResumenDiarioDto
        {
            Fecha = desde,
            MontoInicialCaja = caja?.MontoInicial ?? 0,
            TotalIngresos = ingresosCaja,
            TotalEgresos = egresosCaja,
            // RF-067: Monto final = Monto inicial + Ingresos − Egresos.
            MontoFinalCaja = (caja?.MontoInicial ?? 0) + ingresosCaja - egresosCaja,
            TotalVentas = calc.Total,
            CostoVendido = calc.Costo,
            Ganancia = calc.Ganancia,
            CantidadVentas = calc.Cantidad,
            ProductosVendidos = calc.Unidades,
            CajaAbierta = caja?.Abierta ?? false,
            UsuarioApertura = caja != null
                ? $"{caja.UsuarioApertura.Nombre} {caja.UsuarioApertura.Apellido}"
                : null,
            UsuarioCierre = caja?.UsuarioCierre != null
                ? $"{caja.UsuarioCierre.Nombre} {caja.UsuarioCierre.Apellido}"
                : null
        };

        resumen.VentasPorMetodoPago = ventas
            .Where(v => !v.Anulada)
            .GroupBy(v => v.MetodoPago)
            .Select(g => new VentaPorMetodoDto
            {
                MetodoPago = g.Key,
                Cantidad = g.Count(),
                Total = g.Sum(v => v.Total)
            })
            .OrderBy(v => v.MetodoPago)
            .ToList();

        return resumen;
    }

    // ── RF-059 / RF-060 ────────────────────────────────────────────────────

    public async Task<InformeVentasDto> GetVentasPeriodoAsync(
        DateTime desde, DateTime hasta, int? usuarioId, int? clienteId, string? metodoPago)
    {
        var (d, h) = Rango(desde, hasta);

        var ventas = (await _ventaRepository.GetByRangoFechasConDetallesAsync(d, h))
            .Where(v => !v.Anulada && v.Fecha >= d && v.Fecha < h)
            .AsEnumerable();

        if (usuarioId.HasValue)
            ventas = ventas.Where(v => v.UsuarioId == usuarioId.Value);
        if (clienteId.HasValue)
            ventas = ventas.Where(v => v.ClienteId == clienteId.Value);
        if (!string.IsNullOrWhiteSpace(metodoPago))
            ventas = ventas.Where(v => v.MetodoPago == metodoPago);

        var lista = ventas.ToList();
        var calc = CalcularVentas(lista);

        return new InformeVentasDto
        {
            Desde = d,
            Hasta = h.AddDays(-1),
            Cantidad = calc.Cantidad,
            TotalVendido = calc.Total,
            CostoVendido = calc.Costo,
            Ganancia = calc.Ganancia,
            Ventas = lista.Select(v => v.ToDto()).ToList()
        };
    }

    // ── RF-066 ─────────────────────────────────────────────────────────────

    public async Task<InformeGananciaDto> GetGananciaPeriodoAsync(DateTime desde, DateTime hasta)
    {
        var (d, h) = Rango(desde, hasta);

        var ventas = (await _ventaRepository.GetByRangoFechasConDetallesAsync(d, h))
            .Where(v => !v.Anulada && v.Fecha >= d && v.Fecha < h)
            .ToList();
        var calc = CalcularVentas(ventas);

        return new InformeGananciaDto
        {
            Desde = d,
            Hasta = h.AddDays(-1),
            TotalVendido = calc.Total,
            CostoVendido = calc.Costo,
            Ganancia = calc.Ganancia
        };
    }

    // ── RF-063 ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ProductoVendidoDto>> GetMasVendidosAsync(
        DateTime desde, DateTime hasta, int? top)
    {
        var (d, h) = Rango(desde, hasta);

        var ventas = (await _ventaRepository.GetByRangoFechasConDetallesAsync(d, h))
            .Where(v => !v.Anulada && v.Fecha >= d && v.Fecha < h);

        var ranking = ventas
            .SelectMany(v => v.DetallesVenta)
            .GroupBy(det => det.Producto)
            .Select(g => new ProductoVendidoDto
            {
                ProductoId = g.Key?.Id ?? 0,
                ProductoNombre = g.Key?.Nombre ?? string.Empty,
                ProductoCodigo = g.Key?.Codigo ?? string.Empty,
                UnidadesVendidas = g.Sum(det => det.Cantidad),
                TotalVendido = g.Sum(det => det.Subtotal)
            })
            .OrderByDescending(p => p.UnidadesVendidas)
            .ThenByDescending(p => p.TotalVendido)
            .AsEnumerable();

        if (top.HasValue && top.Value > 0)
            ranking = ranking.Take(top.Value);

        return ranking;
    }

    // ── RF-062 ─────────────────────────────────────────────────────────────

    public async Task<IEnumerable<CajaDto>> GetCajaPeriodoAsync(DateTime desde, DateTime hasta)
    {
        var (d, h) = Rango(desde, hasta);
        var cajas = await _cajaRepository.GetByRangoFechasAsync(d, h.AddDays(-1));
        return cajas.Select(c => c.ToDto());
    }

    // ── RF-072 / RNF-059: exportación ──────────────────────────────────────

    public async Task<byte[]> ExportarAsync(string tipo, DateTime desde, DateTime hasta)
    {
        var (d, h) = Rango(desde, hasta);
        var titulo = $"{tipo} ({d:dd/MM/yyyy} - {h.AddDays(-1):dd/MM/yyyy})";

        switch (tipo.ToLowerInvariant())
        {
            case "ventas":
            {
                var informe = await GetVentasPeriodoAsync(d, h.AddDays(-1), null, null, null);
                return _excel.ExportVentas(titulo, informe.Ventas);
            }
            case "productos":
                return _excel.ExportProductos(titulo, (await _productoRepository.GetAllAsync()).Select(p => p.ToDto()));
            case "stock":
                return _excel.ExportStock(titulo, (await _stockRepository.GetAllAsync()).Select(s => s.ToDto()));
            case "movimientos-caja":
            {
                var movimientos = (await _cajaRepository.GetByRangoFechasAsync(d, h.AddDays(-1)))
                    .SelectMany(c => c.Movimientos ?? Enumerable.Empty<MovimientoCaja>())
                    .OrderByDescending(m => m.Fecha)
                    .Select(m => m.ToDto());
                return _excel.ExportMovimientosCaja(titulo, movimientos);
            }
            case "ingresos-mercaderia":
                return _excel.ExportIngresosMercaderia(
                    titulo, (await _ingresoRepository.GetByRangoFechasAsync(d, h)).Select(i => i.ToDto()));
            case "informes-diarios":
            {
                // RF-068: una fila de resumen por día del período (RF-070).
                var resumenes = new List<ResumenDiarioDto>();
                for (var dia = d; dia < h; dia = dia.AddDays(1))
                    resumenes.Add(await GetResumenDiarioAsync(dia));
                return _excel.ExportResumenDiarios(titulo, resumenes);
            }
            case "ganancias":
            {
                // RF-066: una fila de ganancia por día del período.
                var ganancias = new List<InformeGananciaDto>();
                for (var dia = d; dia < h; dia = dia.AddDays(1))
                    ganancias.Add(await GetGananciaPeriodoAsync(dia, dia));
                return _excel.ExportGanancias(titulo, ganancias);
            }
            default:
                throw new BusinessRuleException(
                    $"Tipo de informe no soportado: '{tipo}'. " +
                    "Tipos válidos: ventas, productos, stock, movimientos-caja, ingresos-mercaderia, informes-diarios, ganancias.");
        }
    }
}
