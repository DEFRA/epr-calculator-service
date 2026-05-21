using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IPrepareCalcService
    {
        Task<bool> PrepareCalcResultsAsync(CalcResultsRequestDto resultsRequestDto, string? runName, CancellationToken cancellationToken);

        Task<bool> PrepareBillingResultsAsync(CalcResultsRequestDto resultsRequestDto, string runName, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Service for preparing calculation results.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public class PrepareCalcService(
        ApplicationDBContext dbContext,
        IMaterialService materialService,
        ICalcResultBuilder builder,
        IStorageService storageService,
        CalculatorRunValidator validator,
        ICalcResultsExporter csvResultsExporter,
        IBillingFileExporter csvBillingExporter,
        ICalcBillingJsonExporter jsonBillingExporter,
        IPrepareProducerDataInsertService producerDataInsertService,
        IOptions<BlobStorageOptions> blobStorageOptions,
        ITelemetryClient telemetry,
        ILogger<PrepareCalcService> logger)
        : IPrepareCalcService
    {
        private const bool OverwriteJsonFile = true;

        private const bool OverwriteCsvFile = false;

        public async Task<bool> PrepareCalcResultsAsync(CalcResultsRequestDto resultsRequestDto, string? runName, CancellationToken cancellationToken)
        {
            CalculatorRun? calculatorRun = null;
            try
            {
                calculatorRun = await dbContext.CalculatorRuns.SingleOrDefaultAsync(
                    run => run.Id == resultsRequestDto.RunId,
                    cancellationToken);

                if (calculatorRun == null)
                {
                    return false;
                }

                // Validate the result for all the required IDs
                var validationResult = validator.ValidateCalculatorRunIds(calculatorRun);
                if (!validationResult.IsValid)
                {
                    return false;
                }

                var materials = await materialService.GetMaterials();
                var results = await telemetry.TrackDuration("CalculatorRunBuilder", () => builder.BuildAsync(resultsRequestDto, materials));
                await logger.LogDuration(() => producerDataInsertService.InsertProducerDataToDatabase(results, materials), "Insert producer data");
                var csvResults = logger.LogDuration(() => csvResultsExporter.Export(results, materials), "Export results");

                var fileName = new CalcResultsAndBillingFileName(
                    results.CalcResultDetail.RunId,
                    results.CalcResultDetail.RunName,
                    results.CalcResultDetail.RunDate);

                string containerName = blobStorageOptions.Value.ResultFileCsvContainer;

                var blobUri = await storageService.UploadFileContentAsync(
                    (FileName: fileName,
                    Content: csvResults,
                    RunName: runName ?? string.Empty,
                    ContainerName: containerName,
                    Overwrite: OverwriteCsvFile));

                if (!string.IsNullOrEmpty(blobUri))
                {
                    await SaveCsvFileMetadataAsync(results.CalcResultDetail.RunId, fileName.ToString(), blobUri);
                    // To fix the operation cancelled issue while updating the context
                    calculatorRun = await dbContext.CalculatorRuns.SingleOrDefaultAsync(
                            run => run.Id == resultsRequestDto.RunId,
                            cancellationToken);
                    calculatorRun!.CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED;
                    dbContext.CalculatorRuns.Update(calculatorRun);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    return true;
                }
            }
            catch (OperationCanceledException exception)
            {
                logger.LogError(exception, "Operation cancelled");
                await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                return false;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred");
                await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                return false;
            }

            logger.LogError("Error occurred");
            await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
            return false;
        }

        public async Task<bool> PrepareBillingResultsAsync(CalcResultsRequestDto resultsRequestDto, string runName, CancellationToken cancellationToken)
        {
            var materials = await materialService.GetMaterials();
            var results = await telemetry.TrackDuration("BillingRunBuilder", () => builder.BuildAsync(resultsRequestDto, materials));

            // Get File name for the billing json file
            var csvName = new CalcResultsAndBillingFileName(resultsRequestDto.RunId, runName, DateTime.UtcNow, true);
            var csvContent = csvBillingExporter.Export(results, materials, resultsRequestDto.AcceptedProducerIds);

            var csvBlobUri = await storageService.UploadFileContentAsync((
                 FileName: csvName,
                 Content: csvContent,
                 RunName: runName,
                 ContainerName: blobStorageOptions.Value.BillingFileCsvContainer,
                 Overwrite: OverwriteCsvFile));

            var csvFileMetaData = new CalculatorRunCsvFileMetadata
            {
                BlobUri = csvBlobUri,
                CalculatorRunId = resultsRequestDto.RunId,
                FileName = csvName
            };

            dbContext.CalculatorRunCsvFileMetadata.Add(csvFileMetaData);

            logger.LogInformation("Exported Billing CSV {BillingCsvFile}", csvName);

            var jsonName = new CalcResultsAndBillingFileName(resultsRequestDto.RunId, true, true);
            var jsonContent = jsonBillingExporter.Export(results, materials, resultsRequestDto.AcceptedProducerIds);

            await storageService.UploadFileContentAsync((
                FileName: jsonName,
                Content: jsonContent,
                RunName: runName,
                ContainerName: blobStorageOptions.Value.BillingFileJsonContainer,
                Overwrite: OverwriteJsonFile));

            var billingFileMetadata = new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = csvName.ToString(),
                BillingFileCreatedBy = resultsRequestDto.ApprovedBy,
                CalculatorRunId = resultsRequestDto.RunId,
                BillingFileCreatedDate = DateTime.UtcNow,
                BillingJsonFileName = jsonName.ToString(),
            };

            dbContext.CalculatorRunBillingFileMetadata.Add(billingFileMetadata);

            logger.LogInformation("Exported Billing JSON {BillingJsonFile}", jsonName);

            var calcRun = await dbContext.CalculatorRuns.SingleAsync(run => run.Id == resultsRequestDto.RunId, cancellationToken);
            calcRun.IsBillingFileGenerating = false;

            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task HandleErrorAsync(CalculatorRun? calculatorRun, RunClassification classification)
        {
            if (calculatorRun != null)
            {
                calculatorRun.CalculatorRunClassificationId = (int)classification;
                dbContext.CalculatorRuns.Update(calculatorRun);
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task SaveCsvFileMetadataAsync(int runId, string fileName, string blobUri)
        {
            var csvFileMetadata = new CalculatorRunCsvFileMetadata
            {
                FileName = fileName,
                BlobUri = blobUri,
                CalculatorRunId = runId,
            };
            await dbContext.CalculatorRunCsvFileMetadata.AddAsync(csvFileMetadata);
        }
    }
}
