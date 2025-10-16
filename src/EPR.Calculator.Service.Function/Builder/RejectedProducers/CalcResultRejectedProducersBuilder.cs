using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.RejectedProducers
{
    public class CalcResultRejectedProducersBuilder : ICalcResultRejectedProducersBuilder
    {
        private readonly ApplicationDBContext context;
        private readonly IProducerDetailService producerDetailsService;

        public CalcResultRejectedProducersBuilder(ApplicationDBContext context,
            IProducerDetailService producerDetailsService
        )
        {
            this.context = context;
            this.producerDetailsService = producerDetailsService;
        }

        public async Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var producersForPreviousRuns = this.producerDetailsService.GetLatestProducerDetailsForThisFinancialYear(resultsRequestDto.FinancialYear);
            var producersForCurrentRun = this.producerDetailsService.GetProducers(resultsRequestDto.RunId);
            var missingProducersInCurrentRun = producersForPreviousRuns
                .Where(t => !producersForCurrentRun.Any(k => k.ProducerId == t.InvoicedTonnage?.ProducerId && t.CalculatorRunId == resultsRequestDto.RunId))
                .DistinctBy(p => p.ProducerDetail?.ProducerId)
                .Select(p => p.ProducerDetail)
                .ToList();            
            var producerDetails = this.context.ProducerDetail
                .AsNoTracking()
                .Where(p => p.CalculatorRunId == resultsRequestDto.RunId)
                .AsEnumerable()
                .DistinctBy(p => p.ProducerId)
                .ToList();

            //Add cancelled producer in current run
            if (missingProducersInCurrentRun.Count > 0)
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
