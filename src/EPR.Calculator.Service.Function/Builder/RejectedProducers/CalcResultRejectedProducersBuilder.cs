using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
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
            this.context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<IEnumerable<CalcResultRejectedProducer>> Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var result = await (from pd in this.context.ProducerDetail.AsNoTracking()
                                join bi in this.context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                                    on new { pd.CalculatorRunId, pd.ProducerId } equals new { bi.CalculatorRunId, bi.ProducerId }
                                where bi.CalculatorRunId == resultsRequestDto.RunId
                                      && bi.BillingInstructionAcceptReject == CommonConstants.Rejected
                                      && bi.ReasonForRejection != null
                                select new CalcResultRejectedProducer
                                {
                                    ProducerId = pd.ProducerId,
                                    ProducerName = pd.ProducerName!,
                                    TradingName = pd.TradingName!,
                                    SuggestedBillingInstruction = bi.SuggestedBillingInstruction,
                                    SuggestedInvoiceAmount = bi.SuggestedInvoiceAmount ?? 0,
                                    InstructionConfirmedDate = bi.LastModifiedAcceptReject,
                                    InstructionConfirmedBy = bi.LastModifiedAcceptRejectBy!,
                                    ReasonForRejection = bi.ReasonForRejection!
                                })
                                .Distinct()
                                .ToListAsync();

            return result ?? new List<CalcResultRejectedProducer>();
        }
    }
}
