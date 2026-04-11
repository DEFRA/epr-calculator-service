using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public class PrepareBillingFileService(
        ApplicationDBContext applicationDBContext,
        IPrepareCalcService prepareCalcService,
        ICalculatorTelemetryLogger telemetryLogger) : IPrepareBillingFileService
    {
        public async Task<bool> PrepareBillingFileAsync(int calculatorRunId, string runName, string approvedBy)
        {
            telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = calculatorRunId,
                RunName = runName,
                Message = "PrepareBillingFileAsync started",
            });

#pragma warning disable S1135
            // TODO We need validate the Run Classification Id But not part of this ticket
#pragma warning restore S1135
            var calculatorRun = await applicationDBContext.CalculatorRuns
            .SingleOrDefaultAsync(x => x.Id == calculatorRunId);

            if (calculatorRun is null)
            {
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calculatorRunId,
                    RunName = runName,
                    Message = PrepareBillingFileConstants.CalculatorRunNotFound,
                });
                return false;
            }

            if (!calculatorRun.IsBillingFileGenerating.GetValueOrDefault())
            {
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calculatorRunId,
                    RunName = runName,
                    Message = PrepareBillingFileConstants.IsBillingFileGeneratingNotSet,
                });
                return false;
            }

            var acceptedProducerIds = await GetAcceptedProducerIdsAsync(calculatorRunId);

            if (acceptedProducerIds.Count == 0)
            {
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calculatorRunId,
                    RunName = runName,
                    Message = PrepareBillingFileConstants.AcceptedProducerIdsAreNull,
                });
                return false;
            }

            var result = await prepareCalcService.PrepareBillingResultsAsync(
                new CalcResultsRequestDto
                {
                    RunId = calculatorRunId,
                    AcceptedProducerIds = acceptedProducerIds,
                    IsBillingFile = true,
                    ApprovedBy = approvedBy,
                    RelativeYear = calculatorRun.RelativeYear
                },
                runName,
                CancellationToken.None);

            return result;
        }

        private async Task<ImmutableHashSet<int>> GetAcceptedProducerIdsAsync(int calculatorRunId)
        {
            return await applicationDBContext
                .ProducerResultFileSuggestedBillingInstruction
                .AsNoTracking()
                .Where(x =>
                    x.CalculatorRunId == calculatorRunId
                    && x.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                    && x.SuggestedBillingInstruction.Trim().ToLower() != PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill.Trim().ToLower())
                .Select(x => x.ProducerId)
                .Distinct()
                .ToImmutableHashSetAsync();
        }
    }
}