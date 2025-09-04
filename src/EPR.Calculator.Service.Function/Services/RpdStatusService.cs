namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using FluentValidation;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Service for updating RPD status.
    /// </summary>
    public class RpdStatusService : IRpdStatusService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RpdStatusService"/> class.
        /// </summary>
        /// <param name="config">The application configuration.</param>
        /// <param name="context">The PayCal database context.</param>
        public RpdStatusService(
            IConfigurationService config,
            IDbContextFactory<ApplicationDBContext> context,
            ICommandTimeoutService commandTimeoutService,
            IRpdStatusDataValidator validator,
            IOrgAndPomWrapper wrapper,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.Config = config;
            this.Context = context.CreateDbContext();
            this.CommandTimeoutService = commandTimeoutService;
            this.Validator = validator;
            this.Wrapper = wrapper;
            this.TelemetryLogger = telemetryLogger;
        }

        private ICalculatorTelemetryLogger TelemetryLogger { get; init; }

        private ICommandTimeoutService CommandTimeoutService { get; init; }

        private IConfigurationService Config { get; init; }

        private ApplicationDBContext Context { get; init; }

        private IRpdStatusDataValidator Validator { get; init; }

        private IOrgAndPomWrapper Wrapper { get; init; }

        /// <inheritdoc/>
        public async Task<RunClassification> UpdateRpdStatus(
            int runId,
            string? runName,
            string updatedBy,
            CancellationToken timeout)
        {
            this.TelemetryLogger.LogInformation(new TrackMessage
            {
                RunId = runId,
                RunName = runName,
                Message = $"Updating RPD status...",
            });

            this.CommandTimeoutService.SetCommandTimeout(this.Context.Database);

            var calcRun = await this.Context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == runId,
                timeout);
            var runClassifications = await this.Context.CalculatorRunClassifications
                .ToListAsync(timeout);

            var validationResult = this.Validator.IsValidRun(calcRun, runId, runClassifications);
            if (!validationResult.isValid)
            {
                this.TelemetryLogger.LogError(new ErrorMessage
                {
                    RunId = runId,
                    RunName = runName,
                    Message = validationResult.ToString(),
                    Exception = new ValidationException(validationResult.ToString()),
                });
                throw new ValidationException(validationResult.ToString());
            }

            var vr = this.Validator.IsValidSuccessfulRun(runId);
            if (!vr.isValid)
            {
                this.TelemetryLogger.LogError(new ErrorMessage
                {
                    RunId = runId,
                    RunName = runName,
                    Message = vr.ToString(),
                    Exception = new ValidationException(vr.ToString()),
                });
                throw new ValidationException(vr.ToString());
            }

            string financialYear = calcRun?.FinancialYearId ?? string.Empty;
            var calendarYear = Util.GetCalendarYearFromFinancialYear(financialYear);
            var createdBy = updatedBy;
            using (var transaction = await this.Context.Database.BeginTransactionAsync(timeout))
            {
                try
                {
                    this.TelemetryLogger.LogInformation(new TrackMessage { RunId = runId, RunName = runName, Message = $"Creating run organization and POM for run: {runId}" });
                    var createRunOrgCommand = Util.GetFormattedSqlString("dbo.CreateRunOrganization", runId, calendarYear, createdBy);
                    await this.Wrapper.ExecuteSqlAsync(createRunOrgCommand, timeout);
                    var createRunPomCommand = Util.GetFormattedSqlString("dbo.CreateRunPom", runId, calendarYear, createdBy);
                    await this.Wrapper.ExecuteSqlAsync(createRunPomCommand, timeout);

                    calcRun!.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.RUNNING.ToString()).Id;
                    await this.Context.SaveChangesAsync(timeout);
                    await transaction.CommitAsync(timeout);
                    return RunClassification.RUNNING;
                }
                catch (Exception)
                {
                    this.TelemetryLogger.LogError(new ErrorMessage
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
}