namespace EPR.Calculator.Service.Function.Builder.CancelledProducers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultCancelledProducersBuilder : ICalcResultCancelledProducersBuilder
    {


        private readonly ApplicationDBContext context;

        public CalcResultCancelledProducersBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultCancelledProducersResponse> Construct(CalcResultsRequestDto resultsRequestDto, string financialYear)
        {
            return await Task.Run(() =>
            {
                // Set up Top Header  
                var topHeader = new CalcResultCancelledProducersDto
                {
                    ProducerId_Header = CommonConstants.ProducerId,
                    ProducerName_Header = CommonConstants.ProducerName,
                    TradingName_Header = CommonConstants.TradingName,
                    LastTonnage = new LastTonnage
                    {
                        LastTonnage_Header = CommonConstants.LastTonnage,
                        Aluminium_Header = CommonConstants.Aluminium,
                        FibreComposite_Header = CommonConstants.FibreComposite,
                        Glass_Header = CommonConstants.Glass,
                        PaperOrCard_Header = CommonConstants.PaperOrCard,
                        Plastic_Header = CommonConstants.Plastic,
                        Steel_Header = CommonConstants.Steel,
                        Wood_Header = CommonConstants.Wood,
                        OtherMaterials_Header = CommonConstants.OtherMaterials
                    },
                    LatestInvoice = new LatestInvoice
                    {
                        LatestInvoice_Header = CommonConstants.LatestInvoice,
                        CurrentYearInvoicedTotalToDate_Header = CommonConstants.CurrentYearInvoicedTotalToDate,
                        RunNumber_Header = CommonConstants.RunNumber,
                        RunName_Header = CommonConstants.RunName,
                        BillingInstructionId_Header = CommonConstants.BillingInstructionId
                    },
                };

                var response = new CalcResultCancelledProducersResponse
                {
                    TitleHeader = CommonConstants.CancelledProducers,
                    CancelledProducers = new List<CalcResultCancelledProducersDto> { topHeader }
                };

                GetCancelledProducers(financialYear, resultsRequestDto.RunId);

                return response;
            });
        }

        public IEnumerable<ProducerInvoicedDto> GetLatestProducerDetailsForThisFinancialYear(string financialYear)
        {
                var previousInvoicedNetTonnage =
                            (from calc in context.CalculatorRuns
                             join p in context.ProducerDesignatedRunInvoiceInstruction
                                 on calc.Id equals p.CalculatorRunId
                             join t in context.ProducerInvoicedMaterialNetTonnage
                                 on new { calc.Id, p.ProducerId } equals new { Id = t.CalculatorRunId, t.ProducerId }
                             where new int[]
                             {
                             RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                             RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRUNCOMPLETEDID
                             }.Contains(calc.CalculatorRunClassificationId) && calc.FinancialYearId == financialYear
                             select new { calc, p, t })
                            .AsEnumerable()
                            .GroupBy(x => new { x.p.ProducerId, x.t.MaterialId })
                            .Select(g =>
                            {
                                var latest = g.OrderByDescending(x => x.calc.Id).First();
                                return new ProducerInvoicedDto
                                {
                                    InvoicedTonnage = latest.t,
                                    CalculatorRun = latest.calc,
                                    InvoiceInstruction = latest.p
                                };
                            })
                            .OrderBy(x => x.InvoicedTonnage?.Id)
                            .ThenBy(x => x.InvoicedTonnage?.ProducerId)
                            .ToList();

                return previousInvoicedNetTonnage;
            
        }

        public IEnumerable<ProducerDetailDto> GetProducers(int runId)
        {
            return context.ProducerDetail.Where(t => t.CalculatorRunId == runId).
                 Select(t => new ProducerDetailDto()
                 {
                     ProducerId = t.ProducerId,
                     ProducerName = t.ProducerName,
                 }
                 ).ToList();
        }


        public void GetCancelledProducers(string financialYear, int runId)
        {
            var previousProducers = GetLatestProducerDetailsForThisFinancialYear(financialYear);
            var producersForThisRun = GetProducers(runId);

            var cancelledProducers = previousProducers.Where(t => !producersForThisRun.Any(k => k.ProducerId == t.InvoicedTonnage?.ProducerId));

            var cancelledProducersWithData = previousProducers.Where(g => cancelledProducers.Any(f => f.InvoicedTonnage?.ProducerId == g.InvoicedTonnage?.ProducerId)).ToList();

            var test = new List<CalcResultCancelledProducersDto>();

            //foreach (var item in cancelledProducersWithData)
            //{
            //    test.Add(new CalcResultCancelledProducersDto()
            //    {
            //         LastTonnage = new LastTonnage() { AluminiumValue = } 
            //    })
            //}
        }

    }
}