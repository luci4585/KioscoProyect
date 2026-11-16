using Kiosco.Application.Interfaces;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Adaptador falso de Storage para tests (SDD §30.10: reemplazar adaptadores
/// reales por implementaciones de prueba). Permite validar las operaciones de
/// imágenes de la Etapa 5 sin tocar Firebase Storage.
/// </summary>
public sealed class FakeStorageService : IStorageService
{
    public List<string> UploadedUrls { get; } = new();
    public List<string> DeletedUrls { get; } = new();

    /// <summary>URLs que deben fallar al subirse (para probar manejo de errores).</summary>
    public bool FailUploads { get; set; }

    public Task<string> UploadImageAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (FailUploads)
            throw new InvalidOperationException("Storage no disponible (simulado).");

        var url = $"https://storage.googleapis.com/test-bucket/{UploadedUrls.Count + 1}-{fileName}";
        UploadedUrls.Add(url);
        return Task.FromResult(url);
    }

    public Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        DeletedUrls.Add(imageUrl);
        return Task.CompletedTask;
    }
}
