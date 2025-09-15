namespace EPR.Calculator.Service.Function.Builder.CancelledProducers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Azure.Analytics.Synapse.Artifacts.Models;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultCancelledProducersBuilder : ICalcResultCancelledProducersBuilder
    {
        private readonly ApplicationDBContext context;
        private readonly IMaterialService materialService;
        private List<MaterialDetail> materials;

        public CalcResultCancelledProducersBuilder(ApplicationDBContext context, IMaterialService materialService)
        {
            this.context = context;
            this.materialService = materialService;
            this.materials = new List<MaterialDetail>();
        }

        public async Task<CalcResultCancelledProducersResponse> Construct(CalcResultsRequestDto resultsRequestDto, string financialYear)
        {

            this.materials = this.materialService.GetMaterials().Result;

            return await Task.Run(() =>
            {
                var producers = new List<CalcResultCancelledProducersDto>();
                producers.AddRange(GetCancelledProducers(financialYear, resultsRequestDto.RunId));

                var response = new CalcResultCancelledProducersResponse
                {
                    TitleHeader = CommonConstants.CancelledProducers,
                    CancelledProducers = producers
                };

                return response;
            });
        }

        public IEnumerable<ProducerInvoicedDto> GetLatestProducerDetailsForThisFinancialYear(string financialYear)
        {
            var previousInvoicedNetTonnage =
                        (from calc in context.CalculatorRuns.AsNoTracking()
                         join p in context.ProducerDesignatedRunInvoiceInstruction.AsNoTracking()
                             on calc.Id equals p.CalculatorRunId
                         join pd in context.ProducerDetail.AsNoTracking()
                             on new { calc.Id, p.ProducerId } equals new { Id = pd.CalculatorRunId, pd.ProducerId }
                         join pbs in context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                             on p.ProducerId equals pbs.ProducerId
                         join t in context.ProducerInvoicedMaterialNetTonnage.AsNoTracking()
                         on new { calc.Id, p.ProducerId } equals new { Id = t.CalculatorRunId, t.ProducerId }
                         where new int[]
                         {
                             RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                             RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRUNCOMPLETEDID
                         }.Contains(calc.CalculatorRunClassificationId) && calc.FinancialYearId == financialYear
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

        public IEnumerable<ProducerDetailDto> GetProducers(int runId)
        {
            return context.ProducerDetail.AsNoTracking().Where(t => t.CalculatorRunId == runId).
                 Select(t => new ProducerDetailDto()
                 {
                     ProducerId = t.ProducerId,
                     ProducerName = t.ProducerName,
                     CalculatorRunId = runId,
                     TradingName = t.TradingName,
                 }
                 ).ToList();
        }


        public IEnumerable<CalcResultCancelledProducersDto> GetCancelledProducers(string financialYear, int runId)
        {
            var producersForPreviousRuns = GetLatestProducerDetailsForThisFinancialYear(financialYear);
            var producersForCurrentRun = GetProducers(runId);

            var missingProducersInCurrentRun = producersForPreviousRuns.Where(t => !producersForCurrentRun.Any(k => k.ProducerId == t.InvoicedTonnage?.ProducerId));
               
            var acceptedCancelledProducersForPreviousRuns = GetAcceptedCancelledProducers(financialYear).ToList();

            var filteredProducersWithOutAccepetedProduecersData = missingProducersInCurrentRun.Where(t => !acceptedCancelledProducersForPreviousRuns.Exists(k=>k == t.InvoicedTonnage?.ProducerId)).ToList();

            var disinctMissingProducers = filteredProducersWithOutAccepetedProduecersData.DistinctBy(t => t.InvoicedTonnage?.ProducerId).Select(t => t.InvoicedTonnage?.ProducerId).ToList();


            var calcResultCancelledProducers = new List<CalcResultCancelledProducersDto>();

            foreach (var prods in disinctMissingProducers)
            {
                var producerId = prods is null ? 0 : prods;

                calcResultCancelledProducers.Add(new CalcResultCancelledProducersDto()
                {
                    ProducerIdValue = prods.ToString(),
                    ProducerOrSubsidiaryNameValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.ProducerDetail?.ProducerId == producerId).Select(t => t.ProducerDetail?.ProducerName).FirstOrDefault(),
                    TradingNameValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.ProducerDetail?.ProducerId == producerId).Select(t => t.ProducerDetail?.TradingName).FirstOrDefault(),

                    LastTonnage = new LastTonnage()
                    {
                        AluminiumValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Aluminium), producerId),
                        FibreCompositeValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.FibreComposite), producerId),
                        GlassValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Glass), producerId),
                        PaperOrCardValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.PaperOrCard), producerId),
                        PlasticValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Plastic), producerId),
                        WoodValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Wood), producerId),
                        SteelValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Steel), producerId),
                        OtherMaterialsValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.OtherMaterials), producerId)
                    },
                    LatestInvoice = new LatestInvoice
                    {
                        BillingInstructionIdValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.InvoiceInstruction?.ProducerId == producerId).Select(t => t.InvoiceInstruction?.BillingInstructionId).FirstOrDefault(),
                        RunNameValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.InvoiceInstruction?.ProducerId == producerId    ).Select(t => t.CalculatorName).FirstOrDefault(),
                        RunNumberValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.InvoiceInstruction?.ProducerId == producerId).Select(t => t.CalculatorRunId).FirstOrDefault().ToString(),
                        CurrentYearInvoicedTotalToDateValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.InvoiceInstruction?.ProducerId == producerId).Select(t => t.InvoiceInstruction?.CurrentYearInvoicedTotalAfterThisRun).FirstOrDefault(),
                    }
                });
            }

            return calcResultCancelledProducers;
        }

        private static decimal? GetInvoicedTonnageForMaterials(List<ProducerInvoicedDto> cancelledProducersWithData, int materialId, int? producerId)
        {
            return cancelledProducersWithData.Where(t => t.InvoicedTonnage?.MaterialId == materialId && t.InvoicedTonnage.ProducerId == producerId).Select(k => k.InvoicedTonnage?.InvoicedNetTonnage).FirstOrDefault();
        }

        private int GetMaterialId(string materialName)
        {
            if (materials is not null)
            {
                return materials.First(t => t.Name == materialName).Id;
            }
            return 0;
        }


        private IEnumerable<int> GetAcceptedCancelledProducers(string financialYear)
        {
            var cancelledAcceptedProducers = (from calc in context.CalculatorRuns.AsNoTracking()
                                              join p in context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                                              on calc.Id equals p.CalculatorRunId
                                              where (calc.FinancialYearId == financialYear && p.BillingInstructionAcceptReject != null && p.BillingInstructionAcceptReject == CommonConstants.Accepted
                                              && p.SuggestedBillingInstruction == CommonConstants.Cancel)
                                               && new int[]
                                                     {
                             RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                             RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRUNCOMPLETEDID
                                                     }.Contains(calc.CalculatorRunClassificationId)
                                              select p.ProducerId).AsEnumerable();
            return cancelledAcceptedProducers;
        }
    }
}