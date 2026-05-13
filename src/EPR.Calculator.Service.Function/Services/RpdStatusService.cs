using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Telemetry;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IRpdStatusService
    {
        Task<RunClassification> UpdateRpdStatus(int runId, string? runName, string createdBy, CancellationToken timeout);
    }

    /// <summary>
    /// Service for updating RPD status.
    /// </summary>
    public class RpdStatusService : IRpdStatusService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RpdStatusService"/> class.
        /// </summary>
        public RpdStatusService(
            IDbContextFactory<ApplicationDBContext> context,
            IRpdStatusDataValidator validator,
            ICalculatorTelemetryLogger telemetryLogger,
            ICalculatorRunOrgData calculatorRunOrgData,
            ICalculatorRunPomData calculatorRunPomData)
        {
            Context = context.CreateDbContext();
            Validator = validator;
            TelemetryLogger = telemetryLogger;
            CalculatorRunOrgData = calculatorRunOrgData;
            CalculatorRunPomData = calculatorRunPomData;
        }

        private ICalculatorTelemetryLogger TelemetryLogger { get; init; }

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

            var calcRun = await Context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == runId,
                timeout);
            var runClassifications = await Context.CalculatorRunClassifications
                .ToListAsync(timeout);

            var validationResult = Validator.IsValidRun(calcRun, runId, runClassifications);
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

                calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.RUNNING.ToString()).Id;

                await Context.SaveChangesAsync(timeout);
                await transaction.CommitAsync(timeout);

                return RunClassification.RUNNING;
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
