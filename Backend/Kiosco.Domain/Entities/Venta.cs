namespace Kiosco.Domain.Entities;

public class Venta : BaseEntity
{
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public bool Anulada { get; set; }
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public MovimientoCaja? MovimientoCaja { get; set; }
}
