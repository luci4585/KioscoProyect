using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Kiosco.API.Authorization;

/// <summary>
/// Resuelve dinámicamente las políticas de autorización con prefijo "Permission:"
/// (p. ej. <c>Permission:rubros.gestionar</c> → <see cref="PermissionRequirement"/>).
/// Delega el resto de políticas (default y fallback) al proveedor por defecto.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(RequirePermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
        {
            var codigo = policyName[RequirePermissionAttribute.PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(codigo));

            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return _fallback.GetPolicyAsync(policyName);
    }
}
