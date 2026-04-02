using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            telemetryLogger = deps.TelemetryLogger;
            ConfigService = deps.ConfigService;
            JsonExporter = deps.JsonExporter;
            BillingFileExporter = deps.BillingFileExporter;
            producerDataInsertService = deps.producerDataInsertService;
        }

        private const bool OverwriteJsonFile = true;

        private const bool OverwriteCsvFile = false;

        private readonly ICalculatorTelemetryLogger telemetryLogger;

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
                    return false;
                }

                // Validate the result for all the required IDs
                var validationResult = validatior.ValidateCalculatorRunIds(calculatorRun);
                if (!validationResult.IsValid)
                {
                    return false;
                }

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Builder started...",
                });

                var results = await Builder.BuildAsync(resultsRequestDto);
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Builder end...",
                });

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Create producer data insert service started...",
                });

                await producerDataInsertService.InsertProducerDataToDatabase(results);
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Create producer data insert service end...",
                });

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Exporter started...",
                });

                var exportedResults = Exporter.Export(results);
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Exporter end...",
                });
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Uploader started...",
                });

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

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Uploader end...",
                });
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Csv File saving started...",
                });

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
                    return true;
                }

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Csv File saving end...",
                });
            }
            catch (OperationCanceledException exception)
            {
                await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Operation cancelled",
                    Exception = exception,
                });
                return false;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Error occurred",
                    Exception = exception,
                });
                return false;
            }

            await HandleErrorAsync(calculatorRun, RunClassification.ERROR);
            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Error occurred",
            });
            return false;
        }

        public async Task<bool> PrepareBillingResultsAsync([FromBody] CalcResultsRequestDto resultsRequestDto,
            string runName,
            CancellationToken cancellationToken)
        {

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing Builder started...",
            });

            var calcResults = await Builder.BuildAsync(resultsRequestDto);

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing Builder ended...",
            });

            // Get File name for the billing json file
            var billingFileCsvName = new CalcResultsAndBillingFileName(
                resultsRequestDto.RunId,
                runName,
                DateTime.UtcNow,
                true);

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file CSV file name only is created {billingFileCsvName}",
            });

            var billingFileJsonName = new CalcResultsAndBillingFileName(resultsRequestDto.RunId, true, true);

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file JSON file name only is created {billingFileJsonName}",
            });

            var jsonResponse = JsonExporter.Export(calcResults, resultsRequestDto.AcceptedProducerIds);

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing file JSON content is now created",
            });

            // call csv Exporter

            var exportedResults = BillingFileExporter.Export(calcResults, resultsRequestDto.AcceptedProducerIds);

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file CSV file before upload {billingFileCsvName}",
            });


            var csvBlobUri = await storageService.UploadFileContentAsync((
                 FileName: billingFileCsvName,
                 Content: exportedResults,
                 RunName: runName,
                 ContainerName: ConfigService.BillingFileCSVBlobContainerName,
                 Overwrite: OverwriteCsvFile));

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file CSV file after upload {billingFileCsvName}",
            });

            // upload the csv file to blob storage

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file JSON file before upload {billingFileJsonName}",
            });

            await storageService.UploadFileContentAsync((
                FileName: billingFileJsonName,
                Content: jsonResponse,
                RunName: runName,
                ContainerName: ConfigService.BillingFileJsonBlobContainerName,
                Overwrite: OverwriteJsonFile));

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file JSON file after upload {billingFileJsonName}",
            });

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

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing file All generated and updated successfully",
            });

            return true;
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