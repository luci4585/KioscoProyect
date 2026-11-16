namespace Kiosco.Application.Interfaces;

/// <summary>
/// Puerto de salida para el almacenamiento de imágenes (SDD §30.4, §30.8).
/// El núcleo de aplicación no conoce Firebase Storage: solo este contrato
/// (RNF-035, RNF-036 — adaptador en Infrastructure).
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Sube una imagen y devuelve su URL pública de acceso.
    /// </summary>
    Task<string> UploadImageAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una imagen previamente subida, identificada por su URL pública.
    /// Operación tolerante a ausencia: eliminar algo inexistente no es error
    /// para el caller (la imagen ya no está = estado deseado).
    /// </summary>
    Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
}
