using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services.Telemetry;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    /// <summary>
    /// Service for updating RPD status.
    /// </summary>
    public class RpdStatusService : IRpdStatusService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RpdStatusService"/> class.
        /// </summary>
        public RpdStatusService(
            IConfigurationService config,
            IDbContextFactory<ApplicationDBContext> context,
            ICommandTimeoutService commandTimeoutService,
            IRpdStatusDataValidator validator,
            ICalculatorTelemetryLogger telemetryLogger,
            ICalculatorRunOrgData calculatorRunOrgData,
            ICalculatorRunPomData calculatorRunPomData)
        {
            Config = config;
            Context = context.CreateDbContext();
            CommandTimeoutService = commandTimeoutService;
            Validator = validator;
            TelemetryLogger = telemetryLogger;
            CalculatorRunOrgData = calculatorRunOrgData;
            CalculatorRunPomData = calculatorRunPomData;
        }

        private ICalculatorTelemetryLogger TelemetryLogger { get; init; }

        private ICommandTimeoutService CommandTimeoutService { get; init; }

        private IConfigurationService Config { get; init; }

        private ApplicationDBContext Context { get; init; }

        private IRpdStatusDataValidator Validator { get; init; }

        private ICalculatorRunOrgData CalculatorRunOrgData { get; init; }

        private ICalculatorRunPomData CalculatorRunPomData { get; init; }

        /// <inheritdoc/>
        public async Task<RunClassification> UpdateRpdStatus(
            int runId,
            string? runName,
            string createdBy,
            CancellationToken timeout)
        {
            TelemetryLogger.LogInformation(new TrackMessage
            {
                RunId = runId,
                RunName = runName,
                Message = "Updating RPD status...",
            });

            CommandTimeoutService.SetCommandTimeout(Context.Database);

            var calcRun = await Context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == runId,
                timeout);

            var validationResult = Validator.IsValidRun(calcRun, runId);
            if (!validationResult.isValid)
            {
                TelemetryLogger.LogError(new ErrorMessage
                {
                    RunId = runId,
                    RunName = runName,
                    Message = validationResult.ToString(),
                    Exception = new ValidationException(validationResult.ToString()),
                });
                throw new ValidationException(validationResult.ToString());
            }

            var vr = Validator.IsValidSuccessfulRun(runId);
            if (!vr.isValid)
            {
                TelemetryLogger.LogError(new ErrorMessage
                {
                    RunId = runId,
                    RunName = runName,
                    Message = vr.ToString(),
                    Exception = new ValidationException(vr.ToString()),
                });
                throw new ValidationException(vr.ToString());
            }

            var relativeYear = calcRun!.RelativeYear;

            await using var transaction = await Context.Database.BeginTransactionAsync(timeout);

            try
            {
                TelemetryLogger.LogInformation(new TrackMessage { RunId = runId, RunName = runName, Message = $"Creating run organization and POM for run: {runId}" });
                await CalculatorRunOrgData.LoadOrgDataForCalcRun(runId, relativeYear, createdBy, timeout);
                await CalculatorRunPomData.LoadPomDataForCalcRun(runId, relativeYear, createdBy, timeout);

                calcRun.Classification = RunClassification.Running;

                await Context.SaveChangesAsync(timeout);
                await transaction.CommitAsync(timeout);

                return RunClassification.Running;
            }
            catch (Exception)
            {
                TelemetryLogger.LogError(new ErrorMessage
                {
                    RunId = runId,
                    RunName = runName,
                    Message = "Error updating RPD status",
                    Exception = new Exception("Error updating RPD status"),
                });
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
