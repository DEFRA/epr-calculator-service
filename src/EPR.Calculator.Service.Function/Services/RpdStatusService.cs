namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
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
            ApplicationDBContext context,
            ICommandTimeoutService commandTimeoutService,
            IRpdStatusDataValidator validator,
            IOrgAndPomWrapper wrapper)
        {
            this.Config = config;
            this.Context = context;
            this.CommandTimeoutService = commandTimeoutService;
            this.Validator = validator;
            this.Wrapper = wrapper;
        }

        private ICommandTimeoutService CommandTimeoutService { get; init; }

        private IConfigurationService Config { get; init; }

        private ApplicationDBContext Context { get; init; }

        private IRpdStatusDataValidator Validator { get; init; }

        private IOrgAndPomWrapper Wrapper { get; init; }

        /// <inheritdoc/>
        public async Task<TimeSpan?> UpdateRpdStatus(
            int runId,
            string updatedBy,
            bool isPomSuccessful,
            CancellationToken timeout)
        {
            this.CommandTimeoutService.SetCommandTimeout(this.Context.Database, "RpdStatusCommand");

            var startTime = DateTime.Now;
            var calcRun = await this.Context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == runId,
                timeout);
            var runClassifications = await this.Context.CalculatorRunClassifications
                .ToListAsync(timeout);

            var validationResult = this.Validator.IsValidRun(calcRun, runId, runClassifications);
            if (!validationResult.isValid)
            {
                throw new ValidationException(validationResult.ToString());
            }

            if (!isPomSuccessful && calcRun != null)
            {
                calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.ERROR.ToString()).Id;
                await this.Context.SaveChangesAsync(timeout);
                return null;
            }

            var vr = this.Validator.IsValidSuccessfulRun(runId);
            if (!vr.isValid)
            {
                throw new ValidationException(vr.ToString());
            }

            string financialYear = calcRun?.Financial_Year ?? string.Empty;
            var calendarYear = Util.GetCalendarYearFromFinancialYear(financialYear);
            var createdBy = updatedBy;
            using (var transaction = await this.Context.Database.BeginTransactionAsync(timeout))
            {
                try
                {
                    var createRunOrgCommand = Util.GetFormattedSqlString("dbo.CreateRunOrganization", runId, calendarYear, createdBy);
                    await this.Wrapper.ExecuteSqlAsync(createRunOrgCommand, timeout);
                    var createRunPomCommand = Util.GetFormattedSqlString("dbo.CreateRunPom", runId, calendarYear, createdBy);
                    await this.Wrapper.ExecuteSqlAsync(createRunPomCommand, timeout);

                    calcRun!.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.RUNNING.ToString()).Id;
                    await this.Context.SaveChangesAsync(timeout);
                    await transaction.CommitAsync(timeout);
                    var timeDiff = startTime - DateTime.Now;

                    return timeDiff;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}