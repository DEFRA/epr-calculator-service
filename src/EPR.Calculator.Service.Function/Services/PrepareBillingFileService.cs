using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Services
{
    public class PrepareBillingFileService(
        ApplicationDBContext applicationDBContext, 
        IPrepareCalcService prepareCalcService, 
        ICalculatorTelemetryLogger telemetryLogger) : IPrepareBillingFileService
    {
        public async Task<bool> PrepareBillingFileAsync(int calculatorRunId, string runName)
        {
            // TODO We need validate the Run Classification Id But not part of this ticket
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

            var acceptedProducerIds = await applicationDBContext.ProducerResultFileSuggestedBillingInstruction
            .Where(x => x.CalculatorRunId == calculatorRunId
                    &&
                    x.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted)
            .Select(x => x.ProducerId).Distinct()
            .ToListAsync();

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

            var result = await prepareCalcService.PrepareBillingResults(
                new CalcResultsRequestDto
                {
                    RunId = calculatorRunId,
                    AcceptedProducerIds = acceptedProducerIds,
                    IsBillingFile = true,
                },
                runName,
                CancellationToken.None);

            return result;
        }
    }
}
