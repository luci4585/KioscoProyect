namespace Kiosco.API.Middleware;

/// <summary>
/// Puerto para la validación de Firebase ID Tokens.
/// SDD §30.4 (puertos de salida) y §30.10 (la arquitectura debe permitir reemplazar
/// adaptadores reales por implementaciones de prueba, necesario para los tests de
/// "usuario autenticado" y "token inválido" de la Etapa 3).
/// </summary>
public interface IFirebaseTokenValidator
{
    /// <summary>
    /// Valida el token y devuelve las claims extraídas, o <c>null</c> si el token es inválido.
    /// </summary>
    Task<FirebaseTokenClaims?> ValidateAsync(string idToken);
}

/// <summary>
/// Claims neutralizadas extraídas de un Firebase ID Token válido.
/// </summary>
public class FirebaseTokenClaims
{
    public string Uid { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
