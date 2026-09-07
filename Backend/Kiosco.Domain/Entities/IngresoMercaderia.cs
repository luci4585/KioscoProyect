namespace Kiosco.Domain.Entities;

public class IngresoMercaderia : BaseEntity
{
    public DateTime Fecha { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public string? Observaciones { get; set; }
    public ICollection<DetalleIngresoMercaderia> Detalles { get; set; } = new List<DetalleIngresoMercaderia>();
}
