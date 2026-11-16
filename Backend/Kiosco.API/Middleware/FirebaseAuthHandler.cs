using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Kiosco.API.Middleware;

public class FirebaseAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IFirebaseTokenValidator _tokenValidator;

    public FirebaseAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IFirebaseTokenValidator tokenValidator)
        : base(options, logger, encoder)
    {
        _tokenValidator = tokenValidator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        var authHeader = Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.NoResult();

        var claims = await _tokenValidator.ValidateAsync(token);
        if (claims == null)
            return AuthenticateResult.Fail("Token inválido o expirado");

        var identity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, claims.Uid),
            new("firebase_uid", claims.Uid),
            new(ClaimTypes.Email, claims.Email),
            new(ClaimTypes.Name, claims.Name)
        }, Scheme.Name);

        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
