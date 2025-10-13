using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Misc
{
    public static class ProducerDetailsHelper
    {
        public static IEnumerable<ProducerInvoicedDto> GetLatestProducerDetailsForThisFinancialYear(string financialYear, ApplicationDBContext context)
        {
            var previousInvoicedNetTonnage =
                        (from calc in context.CalculatorRuns.AsNoTracking()
                         join pbs in context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                             on calc.Id equals pbs.CalculatorRunId
                         join p in context.ProducerDesignatedRunInvoiceInstruction.AsNoTracking()
                             on new { calc.Id, pbs.ProducerId } equals new { Id = p.CalculatorRunId, p.ProducerId }
                         join pd in context.ProducerDetail.AsNoTracking()
                             on new { calc.Id, p.ProducerId } equals new { Id = pd.CalculatorRunId, pd.ProducerId }

                         join t in context.ProducerInvoicedMaterialNetTonnage.AsNoTracking()
                         on new { calc.Id, p.ProducerId } equals new { Id = t.CalculatorRunId, t.ProducerId }
                         where new int[]
                         {
                             RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                             RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRUNCOMPLETEDID
                         }.Contains(calc.CalculatorRunClassificationId) && calc.FinancialYearId == financialYear
                         && pbs.BillingInstructionAcceptReject == CommonConstants.Accepted
                         && pd.SubsidiaryId == null
                         select new { calc, p, pd, pbs, t })
                        .AsEnumerable()
                        .GroupBy(x => new { x.p.ProducerId, x.t.MaterialId })
                        .Select(g =>
                        {
                            var latest = g.OrderByDescending(x => x.calc.Id).First();
                            return new ProducerInvoicedDto
                            {
                                InvoicedTonnage = latest.t,
                                CalculatorName = latest.calc.Name,
                                CalculatorRunId = latest.calc.Id,
                                InvoiceInstruction = latest.p,
                                ProducerDetail = latest.pd,
                                ResultFileSuggestedBillingInstruction = latest.pbs,

                            };
                        })
                        .OrderByDescending(x => x.CalculatorRunId)
                        .ThenBy(x => x.InvoicedTonnage?.ProducerId)
                        .ThenBy(x => x.InvoicedTonnage?.MaterialId)
                        .ToList();

            return previousInvoicedNetTonnage;
        }

        public static IEnumerable<ProducerDetailDto> GetProducers(int runId, ApplicationDBContext context)
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
