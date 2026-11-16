using Microsoft.AspNetCore.Authorization;

namespace Kiosco.API.Authorization;

/// <summary>
/// Protege un endpoint exigiendo un permiso específico del rol del usuario (RNF-034).
/// Los códigos de permiso provienen de los seeds de <c>PermisoConfiguration</c>
/// (RF-008: permisos granulares, no roles rígidos).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public string Codigo { get; }

    public RequirePermissionAttribute(string codigo)
    {
        Codigo = codigo;
        Policy = PolicyPrefix + codigo;
    }
}
