namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

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
            this.telemetryLogger = telemetryLogger;
        }

        private ICalculatorTelemetryLogger telemetryLogger { get; init; }

        private ICommandTimeoutService CommandTimeoutService { get; init; }

        private IConfigurationService Config { get; init; }

        private ApplicationDBContext Context { get; init; }

        private IRpdStatusDataValidator Validator { get; init; }

        private IOrgAndPomWrapper Wrapper { get; init; }

        /// <inheritdoc/>
        public async Task<RunClassification> UpdateRpdStatus(
            int runId,
            string runName,
            string updatedBy,
            bool isPomSuccessful,
            CancellationToken timeout)
        {
            this.telemetryLogger.LogInformation(runId.ToString(), runName, $"Updating RPD status for run: {runId}");

            this.CommandTimeoutService.SetCommandTimeout(this.Context.Database);

            var calcRun = await this.Context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == runId,
                timeout);
            var runClassifications = await this.Context.CalculatorRunClassifications
                .ToListAsync(timeout);

            var validationResult = this.Validator.IsValidRun(calcRun, runId, runClassifications);
            if (!validationResult.isValid)
            {
                this.telemetryLogger.LogError(runId.ToString(), runName, validationResult.ToString(), new ValidationException(validationResult.ToString()));
                throw new ValidationException(validationResult.ToString());
            }

            if (!isPomSuccessful && calcRun != null)
            {
                this.telemetryLogger.LogInformation(runId.ToString(), runName, $"POM failed for run: {runId}");
                calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.ERROR.ToString()).Id;
                await this.Context.SaveChangesAsync(timeout);
                return RunClassification.ERROR;
            }

            var vr = this.Validator.IsValidSuccessfulRun(runId);
            if (!vr.isValid)
            {
                this.telemetryLogger.LogError(runId.ToString(), runName, vr.ToString(), new ValidationException(vr.ToString()));
                throw new ValidationException(vr.ToString());
            }

            string financialYear = calcRun?.Financial_Year ?? string.Empty;
            var calendarYear = Util.GetCalendarYearFromFinancialYear(financialYear);
            var createdBy = updatedBy;
            using (var transaction = await this.Context.Database.BeginTransactionAsync(timeout))
            {
                try
                {
                    this.telemetryLogger.LogInformation(runId.ToString(), runName, $"Creating run organization and POM for run: {runId}");
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
                    this.telemetryLogger.LogError(runId.ToString(), runName, "Error updating RPD status", new Exception("Error updating RPD status"));
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}