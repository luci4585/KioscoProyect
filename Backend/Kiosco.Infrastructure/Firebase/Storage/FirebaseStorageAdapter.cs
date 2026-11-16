using System.Net;
using System.Text;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Kiosco.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Kiosco.Infrastructure.Firebase.Storage;

/// <summary>
/// Adaptador real de Firebase Storage vía Google Cloud Storage compatible
/// (RNF-035, RNF-036). Implementa el puerto <see cref="IStorageService"/>
/// definido en la capa de aplicación (SDD §30.8).
///
/// Configuración requerida (nunca valores inventados):
/// <c>Firebase:ServiceAccountJsonBase64</c> (misma credencial que la API)
/// y <c>Firebase:StorageBucket</c>.
/// </summary>
public class FirebaseStorageAdapter : IStorageService
{
    private const string PublicUrlPrefix = "https://storage.googleapis.com/";
    private const string StorageScope = "https://www.googleapis.com/auth/devstorage.full_control";

    private readonly string _bucketName;
    private readonly Lazy<StorageClient> _client;

    public FirebaseStorageAdapter(IConfiguration configuration)
    {
        _bucketName = configuration["Firebase:StorageBucket"]
            ?? throw new InvalidOperationException(
                "Firebase:StorageBucket no está configurado. " +
                "Definir el bucket real de Firebase Storage (RNF-037).");

        _client = new Lazy<StorageClient>(() =>
        {
            var jsonBase64 = configuration["Firebase:ServiceAccountJsonBase64"]
                ?? throw new InvalidOperationException(
                    "Firebase:ServiceAccountJsonBase64 no está configurado.");

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(jsonBase64));

            // CredentialFactory (sin el obsoleto GoogleCredential.FromJson, CS0618):
            // se crea el credential concreto de cuenta de servicio y se envuelve
            // en GoogleCredential vía ToGoogleCredential() antes de acotar el scope.
            var credential = CredentialFactory
                .FromJson<ServiceAccountCredential>(json)
                .ToGoogleCredential()
                .CreateScoped(StorageScope);

            return StorageClient.Create(credential);
        });
    }

    public async Task<string> UploadImageAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var objectName = BuildObjectName(fileName, contentType);

        // Sobrecarga con bucket y nombre explícitos (evita la ambigüedad de
        // Data.Object sin Bucket).
        await _client.Value.UploadObjectAsync(
            _bucketName,
            objectName,
            contentType,
            content,
            new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead },
            cancellationToken);

        return PublicUrlPrefix + _bucketName + "/" + objectName;
    }

    public async Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        var prefix = PublicUrlPrefix + _bucketName + "/";
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            !imageUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            // URL externa o ya inexistente: nada que eliminar en nuestro bucket.
            return;
        }

        var objectName = Uri.UnescapeDataString(imageUrl[prefix.Length..]);

        try
        {
            await _client.Value.DeleteObjectAsync(_bucketName, objectName, cancellationToken: cancellationToken);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            // Idempotente: ya no existe = estado deseado.
        }
    }

    private static string BuildObjectName(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
            extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".bin"
            };

        // Nombre único y sin dependencia del nombre original del cliente.
        return $"productos/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
    }
}
