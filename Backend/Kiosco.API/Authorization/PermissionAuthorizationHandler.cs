using Kiosco.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Kiosco.API.Authorization;

/// <summary>
/// Evalúa si el usuario interno (identificado por el claim de Firebase UID)
/// posee el permiso requerido a través de su rol (RF-007, RF-008, RNF-034).
///
/// Flujo: Firebase UID → Usuario interno → Rol → RolPermisos → Permiso.Codigo.
/// Usuario deshabilitado o sin rol activo → acceso denegado (RF-005).
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public PermissionAuthorizationHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var firebaseUid = context.User.FindFirst("firebase_uid")?.Value;
        if (string.IsNullOrEmpty(firebaseUid))
            return;

        var usuario = await _usuarioRepository.GetWithPermissionsAsync(firebaseUid);

        if (usuario == null || !usuario.Activo)
            return;

        if (usuario.Rol == null || !usuario.Rol.Activo)
            return;

        var tienePermiso = usuario.Rol.RolPermisos != null &&
            usuario.Rol.RolPermisos.Any(rp =>
                rp.Permiso != null &&
                rp.Permiso.Activo &&
                string.Equals(rp.Permiso.Codigo, requirement.Codigo, StringComparison.Ordinal));

        if (tienePermiso)
            context.Succeed(requirement);
    }
}
