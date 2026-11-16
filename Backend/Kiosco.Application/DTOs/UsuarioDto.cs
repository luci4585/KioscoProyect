namespace Kiosco.Application.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirebaseUid { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class UsuarioCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirebaseUid { get; set; } = string.Empty;
    public int RolId { get; set; }
}

public class UsuarioUpdateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Asignación de rol a un usuario (RF-006). Separado de la modificación de
/// datos para exigir el permiso "roles.gestionar" de forma independiente de
/// "usuarios.gestionar" (RF-007, RF-008).
/// </summary>
public class UsuarioRolUpdateDto
{
    public int RolId { get; set; }
}
