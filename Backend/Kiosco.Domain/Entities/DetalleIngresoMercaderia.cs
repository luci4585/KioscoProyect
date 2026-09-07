namespace Kiosco.Domain.Entities;

public class DetalleIngresoMercaderia
{
    public int Id { get; set; }
    public int IngresoMercaderiaId { get; set; }
    public IngresoMercaderia IngresoMercaderia { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}
