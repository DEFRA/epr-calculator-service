namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Exporter;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Builder;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Service for preparing calculation results.
    /// </summary>
    public class PrepareCalcService : IPrepareCalcService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S107:Methods should not have too many parameters",
            Justification = "To be refactored later.")]
        public PrepareCalcService(
            IDbContextFactory<ApplicationDBContext> context,
            ICalcResultBuilder builder,
            ICalcResultsExporter<CalcResult> exporter,
            IStorageService storageService,
            CalculatorRunValidator validationRules,
            ICommandTimeoutService commandTimeoutService,
            ICalculatorTelemetryLogger telemetryLogger,
            IBillingInstructionService billingInstructionService,
            ICalcBillingJsonExporter<CalcResult> jsonExporter,
            IConfigurationService configService,
            IBillingFileExporter<CalcResult> billingFileExporter)
        {
            this.Context = context.CreateDbContext();
            this.Builder = builder;
            this.Exporter = exporter;
            this.storageService = storageService;
            this.validatior = validationRules;
            this.commandTimeoutService = commandTimeoutService;
            this.telemetryLogger = telemetryLogger;
            this.billingInstructionService = billingInstructionService;
            this.ConfigService = configService;
            this.JsonExporter = jsonExporter;
            this.BillingFileExporter = billingFileExporter;
        }

        public const string ContainerNameMissingError = "Container name is missing in configuration.";

        private readonly ICalculatorTelemetryLogger telemetryLogger;

        private ApplicationDBContext Context { get; init; }

        private ICalcResultBuilder Builder { get; init; }

        private ICalcBillingJsonExporter<CalcResult> JsonExporter { get; init; }

        private ICalcResultsExporter<CalcResult> Exporter { get; init; }

        private IStorageService storageService { get; init; }

        private CalculatorRunValidator validatior { get; init; }

        private ICommandTimeoutService commandTimeoutService { get; init; }

        private IBillingInstructionService billingInstructionService { get; init; }

        private IConfigurationService ConfigService { get; init; }

        private IBillingFileExporter<CalcResult> BillingFileExporter { get; init; }

        public async Task<bool> PrepareCalcResults(
            [FromBody] CalcResultsRequestDto resultsRequestDto,
            string runName,
            CancellationToken cancellationToken)
        {
            this.commandTimeoutService.SetCommandTimeout(this.Context.Database);

            CalculatorRun? calculatorRun = null;
            try
            {
                calculatorRun = await this.Context.CalculatorRuns.SingleOrDefaultAsync(
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

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Builder started...",
                });

                var results = await this.Builder.Build(resultsRequestDto);
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Builder end...",
                });

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Create billing instructions started...",
                });

                await this.billingInstructionService.CreateBillingInstructions(results);
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Create billing instructions end...",
                });

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Exporter started...",
                });

                var exportedResults = this.Exporter.Export(results);
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Exporter end...",
                });
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Uploader started...",
                });

                var fileName = new CalcResultsAndBillingFileName(
                    results.CalcResultDetail.RunId,
                    results.CalcResultDetail.RunName,
                    results.CalcResultDetail.RunDate);

                string containerName = this.ConfigService.ResultFileCSVContainerName;

                var blobUri = await this.storageService.UploadFileContentAsync(
                    (FileName: fileName,
                    Content: exportedResults,
                    RunName: runName,
                    ContainerName: containerName));

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Uploader end...",
                });
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Csv File saving started...",
                });

                var startTime = DateTime.Now;
                if (!string.IsNullOrEmpty(blobUri))
                {
                    await SaveCsvFileMetadataAsync(results.CalcResultDetail.RunId, fileName.ToString(), blobUri);
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED;
                    this.Context.CalculatorRuns.Update(calculatorRun);
                    await this.Context.SaveChangesAsync(cancellationToken);
                    var timeDiff = startTime - DateTime.Now;
                    return true;
                }

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Csv File saving end...",
                });
            }
            catch (OperationCanceledException exception)
            {
                await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                this.telemetryLogger.LogError(new ErrorMessage
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
                await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                this.telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = resultsRequestDto.RunId,
                    RunName = runName,
                    Message = "Error occurred",
                    Exception = exception,
                });
                return false;
            }

            await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Error occurred",
            });
            return false;
        }

        public async Task<bool> PrepareBillingResults([FromBody] CalcResultsRequestDto resultsRequestDto,
            string runName,
            CancellationToken cancellationToken)
        {

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing Builder started...",
            });

            var calcResults = await this.Builder.Build(resultsRequestDto);

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = "Billing Builder ended...",
            });

            // Get File name for the billing json file
            var billingFileCsvName = new CalcResultsAndBillingFileName(
                resultsRequestDto.RunId,
                runName,
                DateTime.Now,
                true);

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file CSV file name only is created {billingFileCsvName}",
            });

            var billingFileJsonName = new CalcResultsAndBillingFileName(resultsRequestDto.RunId, true, true);

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file JSON file name only is created {billingFileJsonName}",
            });

            var jsonResponse = this.JsonExporter.Export(calcResults, resultsRequestDto.AcceptedProducerIds);

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file JSON content is now created",
            });

            // call csv Exporter

            var exportedResults = this.BillingFileExporter.Export(calcResults, resultsRequestDto.AcceptedProducerIds);

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file CSV file before upload {billingFileCsvName}",
            });


            var csvBlobUri = await this.storageService.UploadFileContentAsync((
                 FileName: billingFileCsvName,
                 Content: exportedResults,
                 RunName: runName,
                 ContainerName: ConfigService.ResultFileCSVContainerName));

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file CSV file after upload {billingFileCsvName}",
            });

            // upload the csv file to blob storage

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file JSON file before upload {billingFileJsonName}",
            });

            await this.storageService.UploadFileContentAsync((
                FileName: billingFileJsonName,
                Content: jsonResponse,
                RunName: runName,
                ContainerName: ConfigService.BillingFileJsonBlobContainerName));

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file JSON file after upload {billingFileJsonName}",
            });

            var calcRun = await this.Context.CalculatorRuns.SingleAsync(run => run.Id == resultsRequestDto.RunId);
            calcRun.IsBillingFileGenerating = false;


            var billingFileMetadata = new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = billingFileCsvName.ToString(),
                BillingFileCreatedBy = resultsRequestDto.ApprovedBy,
                CalculatorRunId = resultsRequestDto.RunId,
                BillingFileCreatedDate = DateTime.UtcNow,
                BillingJsonFileName = billingFileJsonName.ToString(),
            };

            this.Context.CalculatorRunBillingFileMetadata.Add(billingFileMetadata);

            var csvFileMetaData = new CalculatorRunCsvFileMetadata
            { BlobUri = csvBlobUri, CalculatorRunId = resultsRequestDto.RunId, FileName = billingFileCsvName };

            this.Context.CalculatorRunCsvFileMetadata.Add(csvFileMetaData);

            await this.Context.SaveChangesAsync();

            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = resultsRequestDto.RunId,
                RunName = runName,
                Message = $"Billing file All generated and updated successfully",
            });

            return true;
        }

        private async Task HandleErrorAsync(CalculatorRun? calculatorRun, RunClassification classification)
        {
            if (calculatorRun != null)
            {
                calculatorRun.CalculatorRunClassificationId = (int)classification;
                this.Context.CalculatorRuns.Update(calculatorRun);
                await this.Context.SaveChangesAsync();
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
            await this.Context.CalculatorRunCsvFileMetadata.AddAsync(csvFileMetadata);
        }
    }
}