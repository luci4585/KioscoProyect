namespace Kiosco.Application.DTOs;

/// <summary>
/// Resumen del día (RF-064), indicadores de la jornada (RF-071) e informe de
/// cierre diario (RF-068) en una sola lectura.
/// </summary>
public class ResumenDiarioDto
{
    public DateTime Fecha { get; set; }
    // RF-064 / RF-071
    public decimal MontoInicialCaja { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal MontoFinalCaja { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal CostoVendido { get; set; }
    public decimal Ganancia { get; set; }
    public int CantidadVentas { get; set; }
    public int ProductosVendidos { get; set; }
    public bool CajaAbierta { get; set; }
    // RF-068
    public string? UsuarioApertura { get; set; }
    public string? UsuarioCierre { get; set; }
    public List<VentaPorMetodoDto> VentasPorMetodoPago { get; set; } = new();
}

public class VentaPorMetodoDto
{
    public string MetodoPago { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}

/// <summary>Reporte de ventas de un período con filtros (RF-059, RF-060).</summary>
public class InformeVentasDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int Cantidad { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal CostoVendido { get; set; }
    public decimal Ganancia { get; set; }
    public List<VentaDto> Ventas { get; set; } = new();
}

/// <summary>Ganancia de un período (RF-066, fórmula RF-065).</summary>
public class InformeGananciaDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal CostoVendido { get; set; }
    public decimal Ganancia { get; set; }
}

/// <summary>Producto con mayor cantidad de unidades vendidas (RF-063).</summary>
public class ProductoVendidoDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public int UnidadesVendidas { get; set; }
    public decimal TotalVendido { get; set; }
}
