using Azure.Storage.Blobs.Models;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Common;
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
            IEnumerable<int> allProducerIds = await GetAllProducerIdsForThisYear(resultsRequestDto.FinancialYear);

            var producersForCurrentRun = this.producerDetailsService.GetProducers(resultsRequestDto.RunId);

            var missingProducersIdsInCurrentRun = allProducerIds.Where(t => !producersForCurrentRun.Any(k => k.ProducerId == t));
            var missingProducersInCurrentRun = await this.producerDetailsService.GetLatestProducerDetailsForThisFinancialYear(resultsRequestDto.FinancialYear, missingProducersIdsInCurrentRun);

            var missingProducerDetail = missingProducersInCurrentRun.Where(t => !producersForCurrentRun.Any(k => k.ProducerId == t.InvoicedTonnage?.ProducerId && t.CalculatorRunId == resultsRequestDto.RunId))
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
            if (missingProducerDetail.Count > 0)
            {
                producerDetails.AddRange(missingProducerDetail!);
            }

            var result = (from detail in producerDetails
                          join billing in this.context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                          on detail.ProducerId equals billing.ProducerId
                          join pdd in this.context.CalculatorRunOrganisationDataDetails 
                          on billing.ProducerId equals pdd.OrganisationId
                          where billing.CalculatorRunId == resultsRequestDto.RunId
                               && billing.BillingInstructionAcceptReject == CommonConstants.Rejected
                               && billing.ReasonForRejection != null && billing.ReasonForRejection.Trim() != ""                               
                          select new CalcResultRejectedProducer
                          {
                              ProducerId = pdd.OrganisationId.GetValueOrDefault(),
                              ProducerName = pdd.OrganisationName!,
                              TradingName = pdd.TradingName!,
                              SuggestedBillingInstruction = billing.SuggestedBillingInstruction,
                              SuggestedInvoiceAmount = GetSuggestedInvoiceAmount(billing),
                              InstructionConfirmedDate = billing.LastModifiedAcceptReject,
                              InstructionConfirmedBy = billing.LastModifiedAcceptRejectBy!,
                              ReasonForRejection = billing.ReasonForRejection!
                          })
                          .DistinctBy(p => p.ProducerId)
                          .OrderBy(p => p.ProducerId)
                          .ToList();

            return result;
        }

        private decimal GetSuggestedInvoiceAmount(ProducerResultFileSuggestedBillingInstruction billing)
        {
            if (billing.SuggestedBillingInstruction == CommonConstants.CancelStatus && 
            billing.BillingInstructionAcceptReject ==  CommonConstants.Rejected)
            {
                return billing.CurrentYearInvoiceTotalToDate ?? 0;
            }

            return billing.SuggestedInvoiceAmount ?? 0;
        }       

        private async Task<IEnumerable<int>> GetAllProducerIdsForThisYear(string financialYear)
        {
            return await (from prfb in context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                          join cr in context.CalculatorRuns.AsNoTracking()
                          on prfb.CalculatorRunId equals cr.Id
                          where
                             new int[]
                             {
                                                            RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                                                            RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                                                            RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                                                            RunClassificationStatusIds.FINALRUNCOMPLETEDID
                             }.Contains(cr.CalculatorRunClassificationId) && cr.FinancialYearId == financialYear
                             && prfb.BillingInstructionAcceptReject == CommonConstants.Accepted
                          select prfb.ProducerId).ToListAsync();
        }
    }
}
