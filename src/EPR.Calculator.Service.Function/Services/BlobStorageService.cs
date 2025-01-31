namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Configuration;

    public class BlobStorageService : IStorageService
    {
        public const string BlobStorageSection = "BlobStorage";
        public const string BlobSettingsMissingError = "BlobStorage settings are missing in configuration.";
        public const string ContainerNameMissingError = "Container name is missing in configuration.";
        public const string OctetStream = "application/octet-stream";
        private readonly BlobContainerClient containerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageService"/> class.
        /// </summary>
        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            var settings = configuration.GetSection(BlobStorageSection).Get<BlobStorageSettings>() ??
                throw new ConfigurationErrorsException(BlobSettingsMissingError);
            this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
                throw new ConfigurationErrorsException(ContainerNameMissingError));
        }

        /// <inheritdoc/>
        public async Task<bool> UploadResultFileContentAsync(string fileName, string content)
        {
            try
            {
                var blobClient = this.containerClient.GetBlobClient(fileName);
                var binaryData = BinaryData.FromString(content);
                await blobClient.UploadAsync(binaryData);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}