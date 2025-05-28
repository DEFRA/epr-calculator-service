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

    /// <summary>
    /// Service for handling blob storage operations.
    /// </summary>
    public class BlobStorageService : IStorageService
    {
        public const string BlobConnectionStringMissingError = "BlobStorage settings are missing in configuration.";
        public const string ContainerNameMissingError = "Container name is missing in configuration.";
        public const string AccountNameMissingError = "Account name is missing in configuration.";
        public const string AccountKeyMissingError = "Account name is missing in configuration.";

        private readonly BlobContainerClient containerClient;
        private readonly StorageSharedKeyCredential sharedKeyCredential;
        private readonly ICalculatorTelemetryLogger telemetryLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageService"/> class.
        /// </summary>
        /// <param name="blobServiceClient">The blob service client.</param>
        /// <param name="config">The configuration service.</param>
        /// <param name="telemetryLogger">The telemetry logger.</param>
        /// <exception cref="ConfigurationErrorsException">Thrown when the container name is missing in the configuration.</exception>
        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            IConfigurationService config,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.containerClient = blobServiceClient.GetBlobContainerClient(config.BlobContainerName
                                                                            ?? throw new ConfigurationErrorsException(ContainerNameMissingError));
            this.telemetryLogger = telemetryLogger;
        }

        /// <inheritdoc/>
        public async Task<string> UploadResultFileContentAsync(string fileName, string content, string runName)
        {
            int? runId = int.TryParse(fileName.Split('-')[0], out var id) ? id : (int?)null;
            try
            {
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = runId,
                    RunName = runName,
                    Message = "Upload Blob started...",
                });
                var blobClient = this.containerClient.GetBlobClient(fileName);
                var binaryData = BinaryData.FromString(content);
                await blobClient.UploadAsync(binaryData);
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = runId,
                    RunName = runName,
                    Message = "Upload Blob end...",
                });
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                this.telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = runId,
                    RunName = runName,
                    Message = "Error writing a Blob",
                    Exception = ex,
                });
                return string.Empty;
            }
        }
    }
}