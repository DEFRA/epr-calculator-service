namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Exporter;
    using EPR.Calculator.API.Validators;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Abstractions;

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
            ICalculatorTelemetryLogger telemetryLogger)
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
        }

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

        public async Task<bool> PrepareCalcResults(
            [FromBody] CalcResultsRequestDto resultsRequestDto,
            CancellationToken cancellationToken,
            string runName)
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

                this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Builder started...");
                var results = await this.Builder.Build(resultsRequestDto);
                this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Builder end...");

                this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Exporter started...");
                var exportedResults = this.Exporter.Export(results);
                this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Exporter end...");

                this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Exporter started...");
                var fileName = new CalcResultsFileName(
                    results.CalcResultDetail.RunId,
                    results.CalcResultDetail.RunName,
                    results.CalcResultDetail.RunDate);
                var blobUri = await this.storageService.UploadResultFileContentAsync(fileName, exportedResults, runName);
                this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Exporter end...");

                this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Csv File saving started...");
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

                this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Csv File saving end...");
            }
            catch (OperationCanceledException exception)
            {
                await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                this.telemetryLogger.LogError(resultsRequestDto.RunId.ToString(), runName, "Operation cancelled", exception);
                return false;
            }
            catch (Exception exception)
            {
                await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                this.telemetryLogger.LogError(resultsRequestDto.RunId.ToString(), runName, "Error occurred", exception);
                return false;
            }

            await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
            this.telemetryLogger.LogInformation(resultsRequestDto.RunId.ToString(), runName, "Error occurred");
            return false;
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
