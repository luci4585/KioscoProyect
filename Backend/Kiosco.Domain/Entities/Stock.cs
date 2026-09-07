namespace Kiosco.Domain.Entities;

public class Stock : BaseEntity
{
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
}
