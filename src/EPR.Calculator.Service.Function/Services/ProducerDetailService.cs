using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class ProducerDetailService : IProducerDetailService
    {

        private readonly ApplicationDBContext context;

        public ProducerDetailService(ApplicationDBContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<ProducerInvoicedDto>> GetLatestProducerDetailsForThisFinancialYear(string financialYear, IEnumerable<int> missingProducersIdsInCurrentRun)
        {
            var producerdetails = await (from pd in context.ProducerDetail
                                         join pds in context.ProducerResultFileSuggestedBillingInstruction on new { pd.ProducerId, pd.CalculatorRunId } equals new { pds.ProducerId, pds.CalculatorRunId }
                                         join ins in context.ProducerInvoicedMaterialNetTonnage on new { pd.ProducerId, pd.CalculatorRunId } equals new { ins.ProducerId, ins.CalculatorRunId }
                                         join d in context.ProducerDesignatedRunInvoiceInstruction on new { pd.ProducerId, pd.CalculatorRunId } equals new { d.ProducerId, d.CalculatorRunId }
                                         join c in context.CalculatorRuns on pd.CalculatorRunId equals c.Id
                                         where missingProducersIdsInCurrentRun.Contains(pd.ProducerId)
                                            && c.FinancialYearId == financialYear
                                            && new int[]
                                            {
                                                RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                                                RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                                                RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                                                RunClassificationStatusIds.FINALRUNCOMPLETEDID
                                             }.Contains(c.CalculatorRunClassificationId)
                                            && pds.BillingInstructionAcceptReject == CommonConstants.Accepted
                                         select new ProducerInvoicedDto()
                                         {
                                             CalculatorRunId = c.Id,
                                             CalculatorName = c.Name,
                                             InvoicedTonnage = ins,
                                             InvoiceInstruction = d,
                                             ProducerDetail = pd,
                                             ResultFileSuggestedBillingInstruction = pds
                                         })
                                         .AsNoTracking()
                                         .OrderByDescending(c => c.CalculatorRunId)
                                         .ThenByDescending(c => c.InvoiceInstruction != null ? c.InvoiceInstruction.CalculatorRunId : 0)
                                         .ThenByDescending(c => c.InvoicedTonnage != null ? c.InvoicedTonnage.CalculatorRunId : 0)
                                         .ThenBy(c => c.InvoicedTonnage != null ? c.InvoicedTonnage.ProducerId : 0)
                                         .ThenBy(c => c.InvoicedTonnage != null ? c.InvoicedTonnage.MaterialId : 0)
                                         .ToListAsync();
            return producerdetails;
        }

        public IEnumerable<ProducerDetailDto> GetProducers(int runId)
        {
            return context.ProducerDetail.AsNoTracking().Where(t => t.CalculatorRunId == runId && t.SubsidiaryId == null).
                 Select(t => new ProducerDetailDto()
                 {
                     ProducerId = t.ProducerId,
                     ProducerName = t.ProducerName,
                     CalculatorRunId = runId,
                     TradingName = t.TradingName,
                 }
                 ).ToList();
        }
    }
}
