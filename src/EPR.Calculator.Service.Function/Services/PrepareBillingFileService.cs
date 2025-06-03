using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Services
{
    public class PrepareBillingFileService(ApplicationDBContext applicationDBContext, IPrepareCalcService prepareCalcService) : IPrepareBillingFileService
    {
        public async Task<bool> PrepareBillingFileAsync(int calculatorRunId, string runName)
        {
            var calculatorRun = await applicationDBContext.CalculatorRuns
            .SingleOrDefaultAsync(x => x.Id == calculatorRunId);

            // We need validate the Run Classification Id But not part of this ticket

            if (calculatorRun is null)
            {
                return false;
                //return new ServiceProcessResponseDto
                //{
                //    StatusCode = HttpStatusCode.UnprocessableContent,
                //    Message = ErrorMessages.InvalidRunId,
                //};
            }

            var acceptedProducerIds = await applicationDBContext.ProducerResultFileSuggestedBillingInstruction
            .Where(x => x.CalculatorRunId == calculatorRunId
                    &&
                    x.BillingInstructionAcceptReject == "Accepted")
            .Select(x => x.ProducerId).Distinct()
            .ToListAsync();

            if (acceptedProducerIds.Count == 0)
            {
                return false;
                //return new ServiceProcessResponseDto
                //{
                //    StatusCode = HttpStatusCode.UnprocessableContent,
                //    Message = ErrorMessages.InvalidOrganisationId,
                //};
            }


            //return new ServiceProcessResponseDto
            //{
            //    StatusCode = HttpStatusCode.OK,
            //    Message = "Billing file prepared successfully (stub)."
            //};

            prepareCalcService.PrepareBillingResults(
                new CalcResultsRequestDto
                {
                    RunId = calculatorRunId,
                    AcceptedProducerIds = acceptedProducerIds,
                    IsBillingFile = true,
                },
                runName,
                CancellationToken.None);

            return true;

            // TODO
            //return await _calcResultsFileBuilder.BuildBillingFileAsync(calculatorRunId, rows, CancellationToken.None)
            //    .ContinueWith(t =>
            //    {
            //        if (t.IsFaulted)
            //        {
            //            return new ServiceProcessResponseDto
            //            {
            //                StatusCode = HttpStatusCode.InternalServerError,
            //                Message = t.Exception?.Message ?? "An error occurred while preparing the billing file.",
            //            };
            //        }

            //        return new ServiceProcessResponseDto
            //        {
            //            StatusCode = HttpStatusCode.OK,
            //            Message = "Billing file prepared successfully.",
            //        };
            //    });
        }
    }
}
