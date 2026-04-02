using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    public class PrepareBillingFileService(
        ApplicationDBContext applicationDBContext,
        IPrepareCalcService prepareCalcService,
        ILogger<PrepareBillingFileService> logger) : IPrepareBillingFileService
    {
        public async Task<bool> PrepareBillingFileAsync(BillingRunParams runParams)
        {
            logger.LogInformation("PrepareBillingFileAsync started");

#pragma warning disable S1135
            // TODO We need validate the Run Classification Id But not part of this ticket
#pragma warning restore S1135
            var calculatorRun = await applicationDBContext.CalculatorRuns
            .SingleOrDefaultAsync(x => x.Id == runParams.Id);

            if (calculatorRun is null)
            {
                logger.LogWarning("Billing: {BillingValidation}", PrepareBillingFileConstants.CalculatorRunNotFound);
                return false;
            }

            if (!calculatorRun.IsBillingFileGenerating.GetValueOrDefault())
            {
                logger.LogWarning("Billing: {BillingValidation}", PrepareBillingFileConstants.IsBillingFileGeneratingNotSet);
                return false;
            }

            List<int> acceptedProducerIds = await GetAcceptedProducerIdsAsync(runParams.Id, applicationDBContext);

            if (acceptedProducerIds.Count == 0)
            {
                logger.LogWarning("Billing: {BillingValidation}", PrepareBillingFileConstants.AcceptedProducerIdsAreNull);
                return false;
            }

            var result = await prepareCalcService.PrepareBillingResultsAsync(
                new CalcResultsRequestDto
                {
                    RunId = runParams.Id,
                    AcceptedProducerIds = acceptedProducerIds,
                    IsBillingFile = true,
                    ApprovedBy = runParams.ApprovedBy,
                    RelativeYear = calculatorRun.RelativeYear
                },
                runParams.Name,
                CancellationToken.None);

            return result;
        }

        private static async Task<List<int>> GetAcceptedProducerIdsAsync(int calculatorRunId, ApplicationDBContext applicationDBContext)
        {
            return await applicationDBContext.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
            .Where(x => x.CalculatorRunId == calculatorRunId
                    &&
                    x.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                    && x.SuggestedBillingInstruction.Trim().ToLower() != PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill.Trim().ToLower())
            .Select(x => x.ProducerId).Distinct()
            .ToListAsync();
        }
    }
}