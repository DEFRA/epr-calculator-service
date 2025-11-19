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
            var billingInstructionsQuery =
                from prsbi in context.ProducerResultFileSuggestedBillingInstruction
                join pd in context.ProducerDetail
                    on new { prsbi.ProducerId, prsbi.CalculatorRunId }
                    equals new { pd.ProducerId, pd.CalculatorRunId }
                    into pd_join
                from pd in pd_join.DefaultIfEmpty()
                where prsbi.CalculatorRunId == resultsRequestDto.RunId
                      && prsbi.BillingInstructionAcceptReject == CommonConstants.Rejected
                      && prsbi.ReasonForRejection != null && prsbi.ReasonForRejection.Trim() != ""
                      && pd.SubsidiaryId == null
                select new
                {
                    prsbi.ProducerId,
                    pd.ProducerName,
                    pd.TradingName,
                    prsbi.SuggestedBillingInstruction,
                    prsbi.SuggestedInvoiceAmount,
                    prsbi.LastModifiedAcceptReject,
                    prsbi.LastModifiedAcceptRejectBy,
                    prsbi.ReasonForRejection
                };

            var latestOrgDetailsByFinancialYearQuery =
                from cr in context.CalculatorRuns
                join crodm in context.CalculatorRunOrganisationDataMaster
                    on cr.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in context.CalculatorRunOrganisationDataDetails
                    on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join b in billingInstructionsQuery
                    on crodd.OrganisationId equals b.ProducerId
                where cr.FinancialYearId == resultsRequestDto.FinancialYear
                      && crodd.OrganisationName != null
                group cr by crodd.OrganisationId into g
                select new
                {
                    OrganisationId = g.Key,
                    LatestRunId = g.Max(x => x.Id)
                };

            var rejectedProducersQuery =
                from cr in context.CalculatorRuns
                join crodm in context.CalculatorRunOrganisationDataMaster
                    on cr.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in context.CalculatorRunOrganisationDataDetails
                    on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join b in billingInstructionsQuery
                    on crodd.OrganisationId equals b.ProducerId
                join latest in latestOrgDetailsByFinancialYearQuery
                    on new { OrgId = crodd.OrganisationId ?? 0, cr.Id }
                    equals new { OrgId = latest.OrganisationId ?? 0, Id = latest.LatestRunId }
                select new CalcResultRejectedProducer
                {
                    runId = cr.Id,
                    ProducerId = crodd.OrganisationId ?? 0,
                    ProducerName = crodd.OrganisationName,
                    TradingName = crodd.TradingName,

                    SuggestedBillingInstruction = b.SuggestedBillingInstruction,
                    SuggestedInvoiceAmount = b.SuggestedInvoiceAmount ?? 0m,
                    InstructionConfirmedDate = b.LastModifiedAcceptReject,
                    InstructionConfirmedBy = b.LastModifiedAcceptRejectBy,
                    ReasonForRejection = b.ReasonForRejection
                };


            var result =await rejectedProducersQuery.AsNoTracking().ToListAsync();

            return result;

        }
    }
}
