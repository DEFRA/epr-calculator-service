using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    /// <summary>
    /// Service for preparing calculation results.
    /// </summary>
    public class PrepareCalcService : IPrepareCalcService
    {
        public PrepareCalcService(PrepareCalcServiceDependencies deps)
        {
            Context = deps.Context;
            Builder = deps.Builder;
            Exporter = deps.Exporter;
            storageService = deps.StorageService;
            validatior = deps.ValidationRules;
            commandTimeoutService = deps.CommandTimeoutService;
            logger = deps.Logger;
            ConfigService = deps.ConfigService;
            JsonExporter = deps.JsonExporter;
            BillingFileExporter = deps.BillingFileExporter;
            producerDataInsertService = deps.producerDataInsertService;
        }

        private const bool OverwriteJsonFile = true;

        private const bool OverwriteCsvFile = false;

        private readonly ILogger<PrepareCalcService> logger;

        private ApplicationDBContext Context { get; init; }

        private ICalcResultBuilder Builder { get; init; }

        private ICalcBillingJsonExporter<CalcResult> JsonExporter { get; init; }

        private ICalcResultsExporter<CalcResult> Exporter { get; init; }

        private IStorageService storageService { get; init; }

        private CalculatorRunValidator validatior { get; init; }

        private ICommandTimeoutService commandTimeoutService { get; init; }

        private IConfigurationService ConfigService { get; init; }

        private IBillingFileExporter<CalcResult> BillingFileExporter { get; init; }
        private IPrepareProducerDataInsertService producerDataInsertService { get; init; }

        public async Task<bool> PrepareCalcResultsAsync(
            [FromBody] CalcResultsRequestDto resultsRequestDto,
            string? runName,
            CancellationToken cancellationToken)
        {
            commandTimeoutService.SetCommandTimeout(Context.Database);

            CalculatorRun? calculatorRun = null;
            try
            {
                calculatorRun = await Context.CalculatorRuns.SingleOrDefaultAsync(
                    run => run.Id == resultsRequestDto.RunId,
                    cancellationToken);
                if (calculatorRun == null)
                {
                    logger.LogWarning("Calculator run not found for RunId: {RunId}", resultsRequestDto.RunId);
                    return false;
                }

                // Validate the result for all the required IDs
                var validationResult = validatior.ValidateCalculatorRunIds(calculatorRun);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Validation failed: {ErrorMessages}", string.Join("; ", validationResult.ErrorMessages));
                    return false;
                }

                logger.LogDebug("Builder started");
                var results = await Builder.BuildAsync(resultsRequestDto);
                logger.LogDebug("Builder end");

                logger.LogDebug("Producer data insert service started");

                await producerDataInsertService.InsertProducerDataToDatabase(results);
                logger.LogDebug("Producer data insert service end");

                logger.LogDebug("Exporter started");

                var exportedResults = Exporter.Export(results);
                logger.LogDebug("Exporter end");
                logger.LogDebug("Uploader started");

                var fileName = new CalcResultsAndBillingFileName(
                    results.CalcResultDetail.RunId,
                    results.CalcResultDetail.RunName,
                    results.CalcResultDetail.RunDate);

                string containerName = ConfigService.ResultFileCSVContainerName;

                var blobUri = await storageService.UploadFileContentAsync(
                    (FileName: fileName,
                    Content: exportedResults,
                    RunName: runName ?? string.Empty,
                    ContainerName: containerName,
                    Overwrite: OverwriteCsvFile));

                logger.LogDebug("Uploader end");
                logger.LogDebug("CSV File saving started");

                if (!string.IsNullOrEmpty(blobUri))
                {
                    await SaveCsvFileMetadataAsync(results.CalcResultDetail.RunId, fileName.ToString(), blobUri);
                    // To fix the operation cancelled issue while updating the context
                    calculatorRun = await Context.CalculatorRuns.SingleOrDefaultAsync(
                            run => run.Id == resultsRequestDto.RunId,
                            cancellationToken);
                    calculatorRun!.CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED;
                    Context.CalculatorRuns.Update(calculatorRun);
                    await Context.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Run classification set to {Classification}", RunClassification.UNCLASSIFIED);
                    return true;
                }

                logger.LogWarning("Blob upload returned empty URI — file may not have been saved");
            }
            catch (OperationCanceledException exception)
            {
                await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                logger.LogError(exception, "Operation cancelled");
                return false;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                logger.LogError(exception, "Error occurred");
                return false;
            }

            await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
            logger.LogWarning("Blob upload failed — run marked as ERROR");
            return false;
        }

        public async Task<bool> PrepareBillingResultsAsync([FromBody] CalcResultsRequestDto resultsRequestDto,
            string runName,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug("Billing Builder started");

                var calcResults = await Builder.BuildAsync(resultsRequestDto);

                logger.LogDebug("Billing Builder ended");

                // Get File name for the billing json file
                var billingFileCsvName = new CalcResultsAndBillingFileName(
                    resultsRequestDto.RunId,
                    runName,
                    DateTime.UtcNow,
                    true);

                var billingFileJsonName = new CalcResultsAndBillingFileName(resultsRequestDto.RunId, true, true);

                var jsonResponse = JsonExporter.Export(calcResults, resultsRequestDto.AcceptedProducerIds);

                logger.LogDebug("Billing file JSON content created");

                // call csv Exporter

                var exportedResults = BillingFileExporter.Export(calcResults, resultsRequestDto.AcceptedProducerIds);

                var csvBlobUri = await storageService.UploadFileContentAsync((
                     FileName: billingFileCsvName,
                     Content: exportedResults,
                     RunName: runName,
                     ContainerName: ConfigService.BillingFileCSVBlobContainerName,
                     Overwrite: OverwriteCsvFile));

                logger.LogInformation("Billing CSV uploaded: {BillingFileCsvName}", billingFileCsvName);

                await storageService.UploadFileContentAsync((
                    FileName: billingFileJsonName,
                    Content: jsonResponse,
                    RunName: runName,
                    ContainerName: ConfigService.BillingFileJsonBlobContainerName,
                    Overwrite: OverwriteJsonFile));

                logger.LogInformation("Billing JSON uploaded: {BillingFileJsonName}", billingFileJsonName);

                var calcRun = await Context.CalculatorRuns.SingleAsync(run => run.Id == resultsRequestDto.RunId);
                calcRun.IsBillingFileGenerating = false;

                var billingFileMetadata = new CalculatorRunBillingFileMetadata
                {
                    BillingCsvFileName = billingFileCsvName.ToString(),
                    BillingFileCreatedBy = resultsRequestDto.ApprovedBy ?? "System User",
                    CalculatorRunId = resultsRequestDto.RunId,
                    BillingFileCreatedDate = DateTime.UtcNow,
                    BillingJsonFileName = billingFileJsonName.ToString(),
                };

                Context.CalculatorRunBillingFileMetadata.Add(billingFileMetadata);

                var csvFileMetaData = new CalculatorRunCsvFileMetadata
                { BlobUri = csvBlobUri, CalculatorRunId = resultsRequestDto.RunId, FileName = billingFileCsvName };

                Context.CalculatorRunCsvFileMetadata.Add(csvFileMetaData);

                await Context.SaveChangesAsync();

                logger.LogInformation("Billing file all generated and updated successfully");

                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while preparing billing results");
                return false;
            }
        }

        private async Task HandleErrorAsync(CalculatorRun? calculatorRun, RunClassification classification)
        {
            if (calculatorRun != null)
            {
                calculatorRun.CalculatorRunClassificationId = (int)classification;
                Context.CalculatorRuns.Update(calculatorRun);
                await Context.SaveChangesAsync();
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
            await Context.CalculatorRunCsvFileMetadata.AddAsync(csvFileMetadata);
        }
    }
}