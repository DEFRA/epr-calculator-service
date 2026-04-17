using System.Diagnostics;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Features.Billing.FileExports;

/// <summary>
///     Creates the billing files for the Billing Run and exports them.
/// </summary>
public interface IBillingFileExporter
{
    /// <summary>
    ///     Writes CSV/JSON billing files to memory, then uploads them to blob storage.
    /// </summary>
    Task<BillingFileExportResult> Export(BillingRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken);
}

/// <inheritdoc />
public class BillingFileExporter(
    IOptions<BlobStorageOptions> blobStorageOptions,
    IBillingFileCsvWriter csvWriter,
    IBillingFileJsonWriter jsonWriter,
    IStorageService storageService,
    ILogger<BillingFileExporter> logger)
    : IBillingFileExporter
{
    /// <inheritdoc />
    public async Task<BillingFileExportResult> Export(BillingRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var csvMetaData = await ExportCsv(runContext, calcResult, cancellationToken);
        logger.LogDebug($"{nameof(ExportCsv)} Completed. File: {{Filename}}, Elapsed:{{Elapsed}}", csvMetaData.FileName, sw.Elapsed);

        sw.Restart();
        var jsonMetaData = await ExportJson(runContext, calcResult, csvMetaData, cancellationToken);
        logger.LogDebug($"{nameof(ExportJson)} Completed. File: {{Filename}}, Elapsed:{{Elapsed}}", jsonMetaData.BillingJsonFileName, sw.Elapsed);

        return new BillingFileExportResult
        {
            CsvMetadata = csvMetaData,
            JsonMetadata = jsonMetaData
        };
    }

    private async Task<CalculatorRunCsvFileMetadata> ExportCsv(
        BillingRunContext runContext,
        CalcResult calcResults,
        CancellationToken ct)
    {
        var csvFilename = new CalcResultsAndBillingFileName(runContext.RunId, runContext.RunName, runContext.ProcessingStartedAt.UtcDateTime, true);
        var csvContent = csvWriter.WriteToString(runContext, calcResults);

        var csvBlobUri = await storageService.UploadFileAsync((
            FileName: csvFilename,
            Content: csvContent,
            runContext.RunName,
            ContainerName: blobStorageOptions.Value.BillingFileCsvContainer,
            Overwrite: true), ct);

        return new CalculatorRunCsvFileMetadata
        {
            BlobUri = csvBlobUri,
            CalculatorRunId = runContext.RunId,
            FileName = csvFilename
        };
    }

    private async Task<CalculatorRunBillingFileMetadata> ExportJson(
        BillingRunContext runContext,
        CalcResult calcResults,
        CalculatorRunCsvFileMetadata csvMetaData,
        CancellationToken ct)
    {
        var jsonFilename = new CalcResultsAndBillingFileName(runContext.RunId);
        var jsonContent = await jsonWriter.WriteToString(runContext, calcResults);

        await storageService.UploadFileAsync((
            FileName: jsonFilename,
            Content: jsonContent,
            runContext.RunName,
            ContainerName: blobStorageOptions.Value.BillingFileJsonContainer,
            Overwrite: true), ct);

        return new CalculatorRunBillingFileMetadata
        {
            CalculatorRunId = runContext.RunId,
            BillingCsvFileName = csvMetaData.FileName,
            BillingFileCreatedBy = runContext.User,
            BillingFileCreatedDate = runContext.ProcessingStartedAt.UtcDateTime,
            BillingJsonFileName = jsonFilename
        };
    }
}