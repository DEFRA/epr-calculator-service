using System.Configuration;
using Azure.Storage.Blobs;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    /// <summary>
    /// Service for handling blob storage operations.
    /// </summary>
    public class BlobStorageService : IStorageService
    {
        public const string BlobConnectionStringMissingError = "BlobStorage settings are missing in configuration.";
        public const string AccountNameMissingError = "Account name is missing in configuration.";
        public const string AccountKeyMissingError = "Account name is missing in configuration.";

        private readonly ILogger<BlobStorageService> logger;
        private readonly BlobServiceClient blobServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageService"/> class.
        /// </summary>
        /// <param name="blobServiceClient">The blob service client.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ConfigurationErrorsException">Thrown when the container name is missing in the configuration.</exception>
        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            ILogger<BlobStorageService> logger)
        {
            this.blobServiceClient = blobServiceClient;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> UploadFileContentAsync(
            (string FileName, string Content, string RunName, string ContainerName, bool Overwrite) args)
        {
            try
            {
                logger.LogDebug("Upload Blob started. ContentSizeBytes: {ContentSizeBytes}, FileName: {FileName}",
                    args.Content.Length, args.FileName);

                var blobContainerClient = blobServiceClient.GetBlobContainerClient(args.ContainerName);
                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(args.FileName);
                var binaryData = BinaryData.FromString(args.Content);
                await blobClient.UploadAsync(binaryData, args.Overwrite);
                logger.LogDebug("Upload Blob end. FileName: {FileName}", args.FileName);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error writing a Blob. FileName: {FileName}", args.FileName);
                return string.Empty;
            }
        }
    }
}