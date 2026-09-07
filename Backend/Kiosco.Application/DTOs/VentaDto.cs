namespace Kiosco.Application.DTOs;

public class VentaDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public bool Anulada { get; set; }
    public List<DetalleVentaDto> Detalles { get; set; } = new();
}

public class VentaCreateDto
{
    public int? ClienteId { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public List<DetalleVentaCreateDto> Detalles { get; set; } = new();
}

public class DetalleVentaDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class DetalleVentaCreateDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}
