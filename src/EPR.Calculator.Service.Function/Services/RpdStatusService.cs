using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            ILogger<RpdStatusService> logger,
            ICalculatorRunOrgData calculatorRunOrgData,
            ICalculatorRunPomData calculatorRunPomData)
        {
            Config = config;
            Context = context.CreateDbContext();
            CommandTimeoutService = commandTimeoutService;
            Validator = validator;
            Logger = logger;
            CalculatorRunOrgData = calculatorRunOrgData;
            CalculatorRunPomData = calculatorRunPomData;
        }

        private ILogger<RpdStatusService> Logger { get; init; }

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
            Logger.LogInformation("Updating RPD status");

            CommandTimeoutService.SetCommandTimeout(Context.Database);

            var calcRun = await Context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == runId,
                timeout);
            var runClassifications = await Context.CalculatorRunClassifications
                .ToListAsync(timeout);

            var validationResult = Validator.IsValidRun(calcRun, runId, runClassifications);
            if (!validationResult.isValid)
            {
                Logger.LogError(new ValidationException(validationResult.ToString()), "RPD validation failed: {ValidationResult}", validationResult);
                throw new ValidationException(validationResult.ToString());
            }

            var vr = Validator.IsValidSuccessfulRun(runId);
            if (!vr.isValid)
            {
                Logger.LogError(new ValidationException(vr.ToString()), "RPD successful-run validation failed: {ValidationResult}", vr);
                throw new ValidationException(vr.ToString());
            }

            var relativeYear = calcRun!.RelativeYear;

            await using var transaction = await Context.Database.BeginTransactionAsync(timeout);
            
            try
            {
                Logger.LogInformation("Creating run organization and POM data");
                await CalculatorRunOrgData.LoadOrgDataForCalcRun(runId, relativeYear, createdBy, timeout);
                await CalculatorRunPomData.LoadPomDataForCalcRun(runId, relativeYear, createdBy, timeout);

            calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.RUNNING.ToString()).Id;

            await Context.SaveChangesAsync(timeout);
            await transaction.CommitAsync(timeout);

                return RunClassification.RUNNING;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating RPD status");
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}