using System.Threading.Tasks;

namespace SynopsisSI.Services.ListingService.Application.Interfaces.Infrastructure;

// For now, let's assume it might be specific or use a shared one.
// If using a shared one, this file would not be here but in SynopsisSI.Shared.Application or similar.
// For this example, let's assume it's defined here for now if ListingService has specific needs.
// Often, the API Gateway or a dedicated Media Service handles pre-signed URLs.
// If ListingService only stores URLs provided by another service/client, it might not need this directly.
public interface ICloudStorageService
{
    // This is more likely used by the client or API Gateway to get upload URLs.
    // ListingService would typically receive the object keys/URLs after upload.
    // Task<string?> GeneratePresignedUploadUrlAsync(string bucketName, string objectKey, int expirationInSeconds = 3600);

    // ListingService might need to construct full URLs for display if it only stores keys:
    string GetPublicUrl(string objectKey);
}