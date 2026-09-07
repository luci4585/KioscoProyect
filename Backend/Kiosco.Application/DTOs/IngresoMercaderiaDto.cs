namespace Kiosco.Application.DTOs;

public class IngresoMercaderiaDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<DetalleIngresoMercaderiaDto> Detalles { get; set; } = new();
}

public class IngresoMercaderiaCreateDto
{
    public int ProveedorId { get; set; }
    public string? Observaciones { get; set; }
    public List<DetalleIngresoMercaderiaCreateDto> Detalles { get; set; } = new();
}

public class DetalleIngresoMercaderiaDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}

public class DetalleIngresoMercaderiaCreateDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}
