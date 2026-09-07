namespace Kiosco.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirebaseUid { get; set; } = string.Empty;
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<MovimientoCaja> MovimientosCaja { get; set; } = new List<MovimientoCaja>();
}
