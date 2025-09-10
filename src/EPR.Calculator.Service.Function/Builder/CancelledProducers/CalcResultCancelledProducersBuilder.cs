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

                var producers = new List<CalcResultCancelledProducersDto>();
                producers.Add(topHeader);
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
                        (from calc in context.CalculatorRuns
                         join p in context.ProducerDesignatedRunInvoiceInstruction
                             on calc.Id equals p.CalculatorRunId
                         join pd in context.ProducerDetail
                             on new { calc.Id, p.ProducerId } equals new { Id = pd.CalculatorRunId, pd.ProducerId }
                         join pbs in context.ProducerResultFileSuggestedBillingInstruction
                             on p.ProducerId equals pbs.ProducerId
                         join t in context.ProducerInvoicedMaterialNetTonnage
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
                                CalculatorRun = latest.calc,
                                InvoiceInstruction = latest.p,
                                ProducerDetail = latest.pd,
                                ResultFileSuggestedBillingInstruction = latest.pbs,

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

            var filteredProducersWithOutAccepetedProduecersData = missingProducersInCurrentRun.Where(k => !acceptedCancelledProducersForPreviousRuns.Contains(k.InvoicedTonnage.ProducerId)).ToList();

            var disinctMissingProducers = filteredProducersWithOutAccepetedProduecersData.DistinctBy(t => t.InvoicedTonnage?.ProducerId).Select(t => t.InvoicedTonnage?.ProducerId).ToList();


            var calcResultCancelledProducers = new List<CalcResultCancelledProducersDto>();

            foreach (var prods in disinctMissingProducers)
            {
                calcResultCancelledProducers.Add(new CalcResultCancelledProducersDto()
                {
                    ProducerIdValue = prods.ToString(),
                    ProducerOrSubsidiaryNameValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.ProducerDetail?.ProducerId == (int)prods).Select(t => t.ProducerDetail?.ProducerName).FirstOrDefault(),
                    TradingNameValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.ProducerDetail?.ProducerId == (int)prods).Select(t => t.ProducerDetail?.TradingName).FirstOrDefault(),

                    LastTonnage = new LastTonnage()
                    {
                        AluminiumValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Aluminium), (int)prods),
                        FibreCompositeValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.FibreComposite), (int)prods),
                        GlassValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Glass), (int)prods),
                        PaperOrCardValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.PaperOrCard), (int)prods),
                        PlasticValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Plastic), (int)prods),
                        WoodValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Wood), (int)prods),
                        SteelValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.Steel), (int)prods),
                        OtherMaterialsValue = GetInvoicedTonnageForMaterials(filteredProducersWithOutAccepetedProduecersData, GetMaterialId(MaterialNames.OtherMaterials), (int)prods)
                    },
                    LatestInvoice = new LatestInvoice
                    {
                        BillingInstructionIdValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.InvoiceInstruction?.ProducerId == (int)prods).Select(t => t.InvoiceInstruction?.BillingInstructionId).FirstOrDefault(),
                        RunNameValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.InvoiceInstruction?.ProducerId == (int)prods).Select(t => t.CalculatorRun?.Name).FirstOrDefault(),
                        RunNumberValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.InvoiceInstruction?.ProducerId == (int)prods).Select(t => t.CalculatorRun?.Id).FirstOrDefault().ToString(),
                        CurrentYearInvoicedTotalToDateValue = filteredProducersWithOutAccepetedProduecersData.Where(t => t.InvoiceInstruction?.ProducerId == (int)prods).Select(t => t.InvoiceInstruction?.CurrentYearInvoicedTotalAfterThisRun).FirstOrDefault(),
                    }
                });
            }

            return calcResultCancelledProducers;
        }

        private static decimal? GetInvoicedTonnageForMaterials(List<ProducerInvoicedDto> cancelledProducersWithData, int materialId, int producerId)
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
            var cancelledAcceptedProducers = (from calc in context.CalculatorRuns
                                              join p in context.ProducerResultFileSuggestedBillingInstruction
                                              on calc.Id equals p.CalculatorRunId
                                              where (calc.FinancialYearId == financialYear && p.BillingInstructionAcceptReject.ToLowerInvariant() == CommonConstants.Accepted.ToLowerInvariant()
                                              && p.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant())
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