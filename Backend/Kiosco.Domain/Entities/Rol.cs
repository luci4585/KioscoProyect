namespace Kiosco.Domain.Entities;

public class Rol : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
