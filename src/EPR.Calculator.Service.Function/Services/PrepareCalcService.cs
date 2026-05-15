using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IPrepareCalcService
    {
        Task<bool> PrepareCalcResultsAsync([FromBody] CalcResultsRequestDto resultsRequestDto, string? runName, CancellationToken cancellationToken);

        Task<bool> PrepareBillingResultsAsync([FromBody] CalcResultsRequestDto resultsRequestDto, string runName, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Service for preparing calculation results.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public class PrepareCalcService(
        ApplicationDBContext dbContext,
        IMaterialService materialService,
        ICalcResultBuilder builder,
        ICalcResultsExporter exporter,
        IStorageService storageService,
        CalculatorRunValidator validator,
        ICalculatorTelemetryLogger telemetryLogger,
        ICalcBillingJsonExporter jsonExporter,
        IBillingFileExporter billingFileExporter,
        IPrepareProducerDataInsertService producerDataInsertService,
        IOptions<BlobStorageOptions> blobStorageOptions)
        : IPrepareCalcService
    {
        private const bool OverwriteJsonFile = true;

        private const bool OverwriteCsvFile = false;

        public async Task<bool> PrepareCalcResultsAsync(
            [FromBody] CalcResultsRequestDto resultsRequestDto,
            string? runName,
            CancellationToken cancellationToken)
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

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Material service started...",
                });
                var materials = await materialService.GetMaterials();
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Material service ended...",
                });

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Builder started...",
                });

                var results = await builder.BuildAsync(resultsRequestDto, materials);
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

                await producerDataInsertService.InsertProducerDataToDatabase(results, materials);
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

                var exportedResults = exporter.Export(results, materials);
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

                string containerName = blobStorageOptions.Value.ResultFileCsvContainer;

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
                    calculatorRun = await dbContext.CalculatorRuns.SingleOrDefaultAsync(
                            run => run.Id == resultsRequestDto.RunId,
                            cancellationToken);
                    calculatorRun!.CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED;
                    dbContext.CalculatorRuns.Update(calculatorRun);
                    await dbContext.SaveChangesAsync(cancellationToken);
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
                Message = "Material service started...",
            });
            var materials = (await materialService.GetMaterials()).ToImmutableList();
            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Material service ended...",
            });

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing Builder started...",
            });

            var calcResults = await builder.BuildAsync(resultsRequestDto, materials);

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing Builder ended...",
            });

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

            var jsonResponse = jsonExporter.Export(calcResults, materials, resultsRequestDto.AcceptedProducerIds);

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing file JSON content is now created",
            });

            var exportedResults = billingFileExporter.Export(calcResults, materials, resultsRequestDto.AcceptedProducerIds);

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
                 ContainerName: blobStorageOptions.Value.BillingFileCsvContainer,
                 Overwrite: OverwriteCsvFile));

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file CSV file after upload {billingFileCsvName}",
            });

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
                ContainerName: blobStorageOptions.Value.BillingFileJsonContainer,
                Overwrite: OverwriteJsonFile));

            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file JSON file after upload {billingFileJsonName}",
            });

            var calcRun = await dbContext.CalculatorRuns.SingleAsync(run => run.Id == resultsRequestDto.RunId);
            calcRun.IsBillingFileGenerating = false;


            var billingFileMetadata = new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = billingFileCsvName.ToString(),
                BillingFileCreatedBy = resultsRequestDto.ApprovedBy ?? "System User",
                CalculatorRunId = resultsRequestDto.RunId,
                BillingFileCreatedDate = DateTime.UtcNow,
                BillingJsonFileName = billingFileJsonName.ToString(),
            };

            dbContext.CalculatorRunBillingFileMetadata.Add(billingFileMetadata);

            var csvFileMetaData = new CalculatorRunCsvFileMetadata
            { BlobUri = csvBlobUri, CalculatorRunId = resultsRequestDto.RunId, FileName = billingFileCsvName };

            dbContext.CalculatorRunCsvFileMetadata.Add(csvFileMetaData);

            await dbContext.SaveChangesAsync();

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
