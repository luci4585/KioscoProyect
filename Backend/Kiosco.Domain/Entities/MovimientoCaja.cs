namespace Kiosco.Domain.Entities;

public class MovimientoCaja : BaseEntity
{
    public int CajaId { get; set; }
    public Caja Caja { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int? VentaId { get; set; }
    public Venta? Venta { get; set; }
}
