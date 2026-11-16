using Microsoft.AspNetCore.Authorization;

namespace Kiosco.API.Authorization;

/// <summary>
/// Requisito de autorización basado en un código de permiso (RF-008, RNF-033).
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Codigo { get; }

    public PermissionRequirement(string codigo)
    {
        Codigo = codigo;
    }
}
