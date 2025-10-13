using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.RejectedProducers
{
    public class CalcResultRejectedProducersBuilder : ICalcResultRejectedProducersBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultRejectedProducersBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var producersForPreviousRuns = ProducerDetailsHelper.GetLatestProducerDetailsForThisFinancialYear(resultsRequestDto.FinancialYear, context);
            var producersForCurrentRun = ProducerDetailsHelper.GetProducers(resultsRequestDto.RunId, context);
            var missingProducersInCurrentRun = producersForPreviousRuns
                .Where(t => !producersForCurrentRun.Any(k => k.ProducerId == t.InvoicedTonnage?.ProducerId))
                .DistinctBy(p => p.ProducerDetail?.ProducerId)
                .Select(p => p.ProducerDetail);            
            var producerDetails = this.context.ProducerDetail
                .AsNoTracking()
                .AsEnumerable()
                .DistinctBy(p => p.ProducerId)
                .ToList();

            //Add cancelled producer in current run
            if (missingProducersInCurrentRun != null)
            {
                producerDetails.AddRange(missingProducersInCurrentRun!);
            }

            var result = (from detail in producerDetails
                          join billing in this.context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                          on detail.ProducerId equals billing.ProducerId
                          where billing.CalculatorRunId == resultsRequestDto.RunId
                               && billing.BillingInstructionAcceptReject == CommonConstants.Rejected
                               && billing.ReasonForRejection != null && billing.ReasonForRejection.Trim() != ""
                               && detail.SubsidiaryId == null
                          select new CalcResultRejectedProducer
                          {
                              ProducerId = detail.ProducerId,
                              ProducerName = detail.ProducerName!,
                              TradingName = detail.TradingName!,
                              SuggestedBillingInstruction = billing.SuggestedBillingInstruction,
                              SuggestedInvoiceAmount = billing.SuggestedInvoiceAmount ?? 0,
                              InstructionConfirmedDate = billing.LastModifiedAcceptReject,
                              InstructionConfirmedBy = billing.LastModifiedAcceptRejectBy!,
                              ReasonForRejection = billing.ReasonForRejection!
                          })
                          .DistinctBy(p => p.ProducerId)
                          .ToList();

            return result ?? new List<CalcResultRejectedProducer>();
        }
    }
}
