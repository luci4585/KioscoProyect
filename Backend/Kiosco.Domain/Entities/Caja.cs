namespace Kiosco.Domain.Entities;

public class Caja : BaseEntity
{
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal? MontoFinal { get; set; }
    public decimal? Diferencia { get; set; }
    public bool Abierta { get; set; }
    public int UsuarioAperturaId { get; set; }
    public Usuario UsuarioApertura { get; set; } = null!;
    public int? UsuarioCierreId { get; set; }
    public Usuario? UsuarioCierre { get; set; }
    public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
}
