namespace Kiosco.Application.DTOs;

public class StockDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int StockMinimo { get; set; }
    public bool StockBajo { get; set; }
}

/// <summary>Movimiento manual de stock: ingreso (RF-022) o egreso (RF-023).</summary>
public class StockMovimientoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}
