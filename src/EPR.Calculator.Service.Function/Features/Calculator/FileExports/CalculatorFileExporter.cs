using System.Diagnostics;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Features.Calculator.FileExports;

public interface ICalculatorFileExporter
{
    /// <summary>
    ///     Writes CSV calc result file to memory, then uploads it to blob storage.
    /// </summary>
    Task<CalculatorFileExportResult> Export(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken);
}

public class CalculatorFileExporter(
    IOptions<BlobStorageOptions> blobStorageOptions,
    IResultsFileCsvWriter csvWriter,
    IStorageService storageService,
    ILogger<CalculatorFileExporter> logger)
    : ICalculatorFileExporter
{
    public async Task<CalculatorFileExportResult> Export(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var csvMetaData = await ExportCsv(runContext, calcResult, cancellationToken);
        logger.LogDebug($"{nameof(ExportCsv)} Completed. File: {{Filename}}, Elapsed:{{Elapsed}}", csvMetaData.FileName, sw.Elapsed);

        return new CalculatorFileExportResult
        {
            CsvMetadata = csvMetaData
        };
    }

    public async Task<CalculatorRunCsvFileMetadata> ExportCsv(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var content = csvWriter.WriteToString(runContext, calcResult);

        var csvFilename = new CalcResultsAndBillingFileName(
            calcResult.CalcResultDetail.RunId,
            calcResult.CalcResultDetail.RunName,
            calcResult.CalcResultDetail.RunDate);

        var csvBlobUri = await storageService.UploadFileAsync(
            (FileName: csvFilename,
                Content: content,
                runContext.RunName,
                ContainerName: blobStorageOptions.Value.ResultFileCsvContainer,
                Overwrite: false), cancellationToken);


        return new CalculatorRunCsvFileMetadata
        {
            BlobUri = csvBlobUri,
            CalculatorRunId = runContext.RunId,
            FileName = csvFilename
        };
    }
}