namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Interface;

    /// <summary>
    /// Service for handling blob storage operations.
    /// </summary>
    public class BlobStorageService : IStorageService
    {
        public const string BlobConnectionStringMissingError = "BlobStorage settings are missing in configuration.";
        public const string AccountNameMissingError = "Account name is missing in configuration.";
        public const string AccountKeyMissingError = "Account name is missing in configuration.";

        private readonly ICalculatorTelemetryLogger telemetryLogger;
        private readonly BlobServiceClient blobServiceClient;

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
            this.blobServiceClient = blobServiceClient;
            this.telemetryLogger = telemetryLogger;
        }

        /// <inheritdoc/>
        public async Task<string> UploadFileContentAsync(
            (string FileName, string Content, string RunName, string ContainerName, bool Overwrite) args)
        {
            int? runId = int.TryParse(args.FileName.Split('-')[0], out var id) ? id : (int?)null;
            try
            {
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = runId,
                    RunName = args.RunName,
                    Message = "Upload Blob started...",
                });

                var blobContainerClient = blobServiceClient.GetBlobContainerClient(args.ContainerName);
                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(args.FileName);
                var binaryData = BinaryData.FromString(args.Content);
                await blobClient.UploadAsync(binaryData, args.Overwrite);
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = runId,
                    RunName = args.RunName,
                    Message = "Upload Blob end...",
                });
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                this.telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = runId,
                    RunName = args.RunName,
                    Message = "Error writing a Blob",
                    Exception = ex,
                });
                return string.Empty;
            }
        }
    }
}