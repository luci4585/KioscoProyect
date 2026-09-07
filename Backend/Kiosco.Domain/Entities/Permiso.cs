namespace Kiosco.Domain.Entities;

public class Permiso : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
