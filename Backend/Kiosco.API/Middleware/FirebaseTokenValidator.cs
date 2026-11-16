using FirebaseAdmin.Auth;

namespace Kiosco.API.Middleware;

/// <summary>
/// Adaptador real que valida tokens contra Firebase Admin SDK (RNF-024).
/// </summary>
public class FirebaseTokenValidator : IFirebaseTokenValidator
{
    private readonly ILogger<FirebaseTokenValidator> _logger;

    public FirebaseTokenValidator(ILogger<FirebaseTokenValidator> logger)
    {
        _logger = logger;
    }

    public async Task<FirebaseTokenClaims?> ValidateAsync(string idToken)
    {
        try
        {
            var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

            return new FirebaseTokenClaims
            {
                Uid = decoded.Uid,
                Email = decoded.Claims.GetValueOrDefault("email")?.ToString() ?? string.Empty,
                Name = decoded.Claims.GetValueOrDefault("name")?.ToString() ?? string.Empty
            };
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(ex, "Token Firebase inválido");
            return null;
        }
    }
}
