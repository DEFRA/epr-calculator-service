using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IStorageService
        {
            Task<string> UploadFileAsync(
                (string FileName,
                 string Content,
                 string RunName,
                 string ContainerName,
                 bool Overwrite
                )
                args, CancellationToken cancellationToken = default);
        }

    public class BlobStorageService(
        BlobServiceClient blobServiceClient,
        ILogger<BlobStorageService> logger)
        : IStorageService
    {
        public async Task<string> UploadFileAsync(
            (string FileName, string Content, string RunName, string ContainerName, bool Overwrite) args, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Upload Blob started. ContentSizeBytes: {ContentSizeBytes}, FileName: {FileName}",
                args.Content.Length, args.FileName);

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(args.ContainerName);
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = blobContainerClient.GetBlobClient(args.FileName);
            var binaryData = BinaryData.FromString(args.Content);
            await blobClient.UploadAsync(binaryData, args.Overwrite, cancellationToken);
            logger.LogDebug("Upload Blob end. FileName: {FileName}", args.FileName);
            return blobClient.Uri.ToString();
        }
    }
}