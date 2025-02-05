namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Configuration;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Http;

    public class BlobStorageService : IStorageService
    {
        public const string BlobStorageSection = "BlobStorage";
        public const string BlobSettingsMissingError = "BlobStorage settings are missing in configuration.";
        public const string ContainerNameMissingError = "Container name is missing in configuration.";
        public const string AccountNameMissingError = "Account name is missing in configuration.";
        public const string OctetStream = "application/octet-stream";
        private readonly BlobContainerClient containerClient;
        private readonly StorageSharedKeyCredential sharedKeyCredential;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfigurationService config, ILogger<BlobStorageService> logger)
        {

            this.containerClient = blobServiceClient.GetBlobContainerClient(config.BlobContainerName
                                                                            ?? throw new ConfigurationErrorsException(ContainerNameMissingError));

            //var settings = configuration.GetSection(BlobStorageSection).Get<BlobStorageSettings>() ??
            //    throw new ConfigurationErrorsException(BlobSettingsMissingError);

            //settings.ExtractAccountDetails();

            //this.sharedKeyCredential = new StorageSharedKeyCredential(settings.AccountName, settings.AccountKey) ??
            //    throw new ConfigurationErrorsException(AccountNameMissingError);

            //this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
            //    throw new ConfigurationErrorsException(ContainerNameMissingError));

            _logger = logger;
        }

        public async Task<bool> UploadResultFileContentAsync(string fileName, string content)
        {
            try
            {
                var blobClient = this.containerClient.GetBlobClient(fileName);
                var binaryData = BinaryData.FromString(content);
                await blobClient.UploadAsync(binaryData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Blob write");
                return false;
            }
        }

        //public async Task<IResult> DownloadFile(string fileName, string blobUri)
        //{
        //    BlobClient? blobClient = null;

        //    if (!string.IsNullOrEmpty(blobUri))
        //    {
        //        try
        //        {
        //            blobClient = new BlobClient(new Uri(blobUri), sharedKeyCredential);
        //        }
        //        catch (UriFormatException exception)
        //        {
        //            _logger.LogError(exception, "Blob Uri is not in correct format.");
        //            blobClient ??= this.containerClient.GetBlobClient(fileName);
        //        }
        //    }
        //    else
        //        blobClient ??= this.containerClient.GetBlobClient(fileName);

        //    if (!await blobClient.ExistsAsync())
        //    {
        //        return false;
        //    }

        //    try
        //    {
        //        var downloadResult = await blobClient.DownloadContentAsync();
        //        var content = downloadResult.Value.Content.ToString();
        //        return Results.File(Encoding.Unicode.GetBytes(content), OctetStream, fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Results.Problem($"An error occurred while downloading the file: {ex.Message}");
        //    }
        //}
    }
}