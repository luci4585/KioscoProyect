using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

public interface IStockService
{
    Task<IEnumerable<StockDto>> GetAllAsync();
    Task<StockDto?> GetByProductoAsync(int productoId);

    /// <summary>Stocks con cantidad igual o inferior al mínimo (RF-026 — alertas).</summary>
    Task<IEnumerable<StockDto>> GetAlertasAsync();

    /// <summary>Registra un ingreso y actualiza la cantidad disponible (RF-022).</summary>
    Task<StockDto> IngresarAsync(int productoId, int cantidad);

    /// <summary>
    /// Registra un egreso y actualiza la cantidad disponible (RF-023).
    /// Lanza BusinessRuleException si supera el stock disponible (RF-077).
    /// </summary>
    Task<StockDto> EgresarAsync(int productoId, int cantidad);
}
