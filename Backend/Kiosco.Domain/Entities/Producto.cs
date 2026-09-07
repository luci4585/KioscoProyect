namespace Kiosco.Domain.Entities;

public class Producto : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int RubroId { get; set; }
    public Rubro Rubro { get; set; } = null!;
    public decimal PrecioVenta { get; set; }
    public decimal Costo { get; set; }
    public int StockMinimo { get; set; }
    public string? ImagenUrl { get; set; }
    public Stock Stock { get; set; } = null!;
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public ICollection<DetalleIngresoMercaderia> DetallesIngreso { get; set; } = new List<DetalleIngresoMercaderia>();
}
