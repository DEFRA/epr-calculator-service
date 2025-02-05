namespace EPR.Calculator.Service.Function.Services
{
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Configuration;
    using System.Text;
    using System.Threading.Tasks;

    public class BlobStorageService : IStorageService
    {
        public const string BlobConnectionStringMissingError = "BlobStorage settings are missing in configuration.";
        public const string ContainerNameMissingError = "Container name is missing in configuration.";
        public const string AccountNameMissingError = "Account name is missing in configuration.";
        public const string AccountKeyMissingError = "Account name is missing in configuration.";

        private readonly BlobContainerClient containerClient;
        private readonly StorageSharedKeyCredential sharedKeyCredential;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfigurationService config, ILogger<BlobStorageService> logger)
        {
            this.containerClient = blobServiceClient.GetBlobContainerClient(config.BlobContainerName
                                                                            ?? throw new ConfigurationErrorsException(ContainerNameMissingError));
            this._logger = logger;
        }

        public async Task<string> UploadResultFileContentAsync(string fileName, string content)
        {
            try
            {
                var blobClient = this.containerClient.GetBlobClient(fileName);
                var binaryData = BinaryData.FromString(content);
                await blobClient.UploadAsync(binaryData);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Blob write");
                return string.Empty;
            }
        }
    }
}
