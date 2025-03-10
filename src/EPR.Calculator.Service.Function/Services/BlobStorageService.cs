namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    public class BlobStorageService : IStorageService
    {
        public const string BlobConnectionStringMissingError = "BlobStorage settings are missing in configuration.";
        public const string ContainerNameMissingError = "Container name is missing in configuration.";
        public const string AccountNameMissingError = "Account name is missing in configuration.";
        public const string AccountKeyMissingError = "Account name is missing in configuration.";

        private readonly BlobContainerClient containerClient;
        private readonly StorageSharedKeyCredential sharedKeyCredential;
        private readonly ICalculatorTelemetryLogger telemetryLogger;

        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            IConfigurationService config,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.containerClient = blobServiceClient.GetBlobContainerClient(config.BlobContainerName
                                                                            ?? throw new ConfigurationErrorsException(ContainerNameMissingError));
            this.telemetryLogger = telemetryLogger;
        }

        public async Task<string> UploadResultFileContentAsync(string fileName, string content, string runName)
        {
            string runId = fileName.Split('-')[0];
            try
            {
                this.telemetryLogger.LogInformation(runId, runName, "Upload Blob started...");
                var blobClient = this.containerClient.GetBlobClient(fileName);
                var binaryData = BinaryData.FromString(content);
                await blobClient.UploadAsync(binaryData);
                this.telemetryLogger.LogInformation(runId, runName, "Upload Blob end...");
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                this.telemetryLogger.LogError(runId, runName, "Error writing a Blob ", ex);
                return string.Empty;
            }
        }
    }
}
