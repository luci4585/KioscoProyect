namespace Kiosco.Application.DTOs;

public class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int RubroId { get; set; }
    public string RubroNombre { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public decimal Costo { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; }
}

public class ProductoCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int RubroId { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal Costo { get; set; }
    public int StockMinimo { get; set; }
}

public class ProductoUpdateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int RubroId { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal Costo { get; set; }
    public int StockMinimo { get; set; }
}
