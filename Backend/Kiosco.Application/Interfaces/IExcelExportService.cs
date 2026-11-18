using Kiosco.Application.DTOs;

namespace Kiosco.Application.Interfaces;

/// <summary>
/// Generación de archivos Excel en el backend (RNF-058). La biblioteca
/// seleccionada al implementar la Etapa 11 (RNF-057) es **ClosedXML**.
/// </summary>
public interface IExcelExportService
{
    byte[] ExportVentas(string titulo, IEnumerable<VentaDto> ventas);
    byte[] ExportProductos(string titulo, IEnumerable<ProductoDto> productos);
    byte[] ExportStock(string titulo, IEnumerable<StockDto> stock);
    byte[] ExportMovimientosCaja(string titulo, IEnumerable<MovimientoCajaDto> movimientos);
    byte[] ExportIngresosMercaderia(string titulo, IEnumerable<IngresoMercaderiaDto> ingresos);
    byte[] ExportResumenDiarios(string titulo, IEnumerable<ResumenDiarioDto> resumenes);
    byte[] ExportGanancias(string titulo, IEnumerable<InformeGananciaDto> ganancias);
}
