using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IInformeService
{
    /// <summary>Resumen del día (RF-064), indicadores (RF-071) y cierre diario (RF-068).</summary>
    Task<ResumenDiarioDto> GetResumenDiarioAsync(DateTime fecha);

    /// <summary>Ventas del período con filtros (RF-059, RF-060).</summary>
    Task<InformeVentasDto> GetVentasPeriodoAsync(
        DateTime desde, DateTime hasta, int? usuarioId, int? clienteId, string? metodoPago);

    /// <summary>Ganancia del período (RF-066, fórmula RF-065).</summary>
    Task<InformeGananciaDto> GetGananciaPeriodoAsync(DateTime desde, DateTime hasta);

    /// <summary>Productos más vendidos del período (RF-063).</summary>
    Task<IEnumerable<ProductoVendidoDto>> GetMasVendidosAsync(DateTime desde, DateTime hasta, int? top);

    /// <summary>Movimientos y resultados de caja del período (RF-062).</summary>
    Task<IEnumerable<CajaDto>> GetCajaPeriodoAsync(DateTime desde, DateTime hasta);

    /// <summary>Exportación a Excel (RF-072, RNF-057 a RNF-059).</summary>
    Task<byte[]> ExportarAsync(string tipo, DateTime desde, DateTime hasta);
}
