namespace EPR.Calculator.Service.Function.Services
{
    using Azure.Storage.Blobs;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Exporter;
    using EPR.Calculator.API.Validators;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Builder;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Abstractions;
    using System;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for preparing calculation results.
    /// </summary>
    public class PrepareCalcService : IPrepareCalcService
    {
        public PrepareCalcService(
            IDbContextFactory<ApplicationDBContext> context,
            IRpdStatusDataValidator rpdStatusDataValidator,
            IOrgAndPomWrapper wrapper,
            ICalcResultBuilder builder,
            ICalcResultsExporter<CalcResult> exporter,
            ITransposePomAndOrgDataService transposePomAndOrgDataService,
            IStorageService storageService,
            CalculatorRunValidator validationRules,
            ICommandTimeoutService commandTimeoutService,
            ICalculatorTelemetryLogger telemetryLogger,
            IBillingInstructionService billingInstructionService,
            BlobServiceClient blobServiceClient,
            IConfigurationService config)
        {
            this.Context = context.CreateDbContext();
            this.rpdStatusDataValidator = rpdStatusDataValidator;
            this.Wrapper = wrapper;
            this.Builder = builder;
            this.Exporter = exporter;
            this.transposePomAndOrgDataService = transposePomAndOrgDataService;
            this.storageService = storageService;
            this.validatior = validationRules;
            this.commandTimeoutService = commandTimeoutService;
            this.telemetryLogger = telemetryLogger;
            this.billingInstructionService = billingInstructionService;
            this.Config = config;
        }

        public const string ContainerNameMissingError = "Container name is missing in configuration.";

        private readonly ICalculatorTelemetryLogger telemetryLogger;

        private ApplicationDBContext Context { get; init; }

        private IRpdStatusDataValidator rpdStatusDataValidator { get; init; }

        private IOrgAndPomWrapper Wrapper { get; init; }

        private ICalcResultBuilder Builder { get; init; }

        private ICalcResultsExporter<CalcResult> Exporter { get; init; }

        private ITransposePomAndOrgDataService transposePomAndOrgDataService { get; init; }

        private IStorageService storageService { get; init; }

        private CalculatorRunValidator validatior { get; init; }

        private ICommandTimeoutService commandTimeoutService { get; init; }

        private IBillingInstructionService billingInstructionService { get; init; }

        private IConfigurationService Config { get; init; }

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

                var fileName = new CalcResultsFileName(
                    results.CalcResultDetail.RunId,
                    results.CalcResultDetail.RunName,
                    results.CalcResultDetail.RunDate);

                string containerName = this.Config.CalcResultBlobContainerName
                        ?? throw new ConfigurationErrorsException(ContainerNameMissingError);

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

        public Task<bool> PrepareBillingResults([FromBody] CalcResultsRequestDto resultsRequestDto,
            string runName,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException("PrepareBillingResults is not implemented yet.");
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