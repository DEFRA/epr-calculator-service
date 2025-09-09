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

                var response = new CalcResultCancelledProducersResponse
                {
                    TitleHeader = CommonConstants.CancelledProducers,
                    CancelledProducers = GetCancelledProducers(financialYear, resultsRequestDto.RunId)
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
                                 on p.ProducerId equals pd.ProducerId
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
                             select new { calc, p, t, pd, pbs })
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
                 }
                 ).ToList();
        }


        public IEnumerable<CalcResultCancelledProducersDto> GetCancelledProducers(string financialYear, int runId)
        {
            var previousProducers = GetLatestProducerDetailsForThisFinancialYear(financialYear);
            var producersForThisRun = GetProducers(runId);

            var missingProducers = previousProducers.Where(t => !producersForThisRun.Any(k => k.ProducerId == t.InvoicedTonnage?.ProducerId));

            var missingProducersWithData = previousProducers.Where(g => missingProducers.Any(f => f.InvoicedTonnage?.ProducerId == g.InvoicedTonnage?.ProducerId)).ToList();
            var acceptedCancelledProducers = GetAcceptedCancelledProducers(financialYear).ToList();

            var filteredProducersWithOutAccepetedProduecersData = missingProducersWithData.Where(k => !acceptedCancelledProducers.Contains(k.InvoicedTonnage.ProducerId)).ToList();

            var disinctMissingProducers = filteredProducersWithOutAccepetedProduecersData.DistinctBy(t =>  t.InvoicedTonnage?.ProducerId).Select(t => t.InvoicedTonnage?.ProducerId).ToList();
           

            var calcResultCancelledProducers = new List<CalcResultCancelledProducersDto>();

            foreach (var prods  in disinctMissingProducers)
            {
                calcResultCancelledProducers.Add(new CalcResultCancelledProducersDto()
                {
                    ProducerIdValue = prods.ToString(),
                    ProducerOrSubsidiaryNameValue = missingProducersWithData.Where(t => t.ProducerDetail?.ProducerId == (int)prods).Select(t => t.ProducerDetail?.ProducerName).FirstOrDefault(),
                     TradingNameValue = missingProducersWithData.Where(t => t.ProducerDetail?.ProducerId == (int)prods).Select(t => t.ProducerDetail?.TradingName).FirstOrDefault(),

                    LastTonnage = new LastTonnage()
                    {
                        AluminiumValue = GetInvoicedTonnageForMaterials(missingProducersWithData, GetMaterialId(MaterialNames.Aluminium), (int)prods),
                        FibreCompositeValue = GetInvoicedTonnageForMaterials(missingProducersWithData, GetMaterialId(MaterialNames.FibreComposite), (int)prods),
                        GlassValue = GetInvoicedTonnageForMaterials(missingProducersWithData, GetMaterialId(MaterialNames.Glass), (int)prods),
                        PaperOrCardValue = GetInvoicedTonnageForMaterials(missingProducersWithData, GetMaterialId(MaterialNames.PaperOrCard), (int)prods),
                        PlasticValue = GetInvoicedTonnageForMaterials(missingProducersWithData, GetMaterialId(MaterialNames.Plastic), (int)prods),
                        WoodValue = GetInvoicedTonnageForMaterials(missingProducersWithData, GetMaterialId(MaterialNames.Wood), (int)prods),
                        SteelValue = GetInvoicedTonnageForMaterials(missingProducersWithData, GetMaterialId(MaterialNames.Steel), (int)prods),
                        OtherMaterialsValue = GetInvoicedTonnageForMaterials(missingProducersWithData, GetMaterialId(MaterialNames.OtherMaterials), (int)prods)
                    },
                    LatestInvoice = new LatestInvoice
                    {
                        BillingInstructionIdValue = missingProducersWithData.Where(t => t.InvoiceInstruction?.ProducerId == (int)prods).Select(t => t.InvoiceInstruction?.BillingInstructionId).FirstOrDefault(),
                        RunNameValue = missingProducersWithData.Where(t => t.InvoiceInstruction?.ProducerId == (int)prods).Select(t => t.CalculatorRun?.Name).FirstOrDefault(),
                        RunNumberValue = missingProducersWithData.Where(t => t.InvoiceInstruction?.ProducerId == (int)prods).Select(t => t.CalculatorRun?.Id).FirstOrDefault().ToString(),
                        CurrentYearInvoicedTotalToDateValue = missingProducersWithData.Where(t => t.InvoiceInstruction?.ProducerId == (int)prods).Select(t => t.InvoiceInstruction?.CurrentYearInvoicedTotalAfterThisRun).FirstOrDefault(),
                    }
                });
            }

            return calcResultCancelledProducers;
        }

        private static decimal? GetInvoicedTonnageForMaterials(List<ProducerInvoicedDto> cancelledProducersWithData, int materialId, int producerId)
        {
            return cancelledProducersWithData.Where(t => t.InvoicedTonnage?.MaterialId ==materialId && t.InvoicedTonnage.ProducerId == producerId).Select(k => k.InvoicedTonnage?.InvoicedNetTonnage).FirstOrDefault();
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
            var test = (from calc in context.CalculatorRuns
                       join p in context.ProducerResultFileSuggestedBillingInstruction
                       on calc.Id equals p.CalculatorRunId
                       where (calc.FinancialYearId == financialYear && p.BillingInstructionAcceptReject == "Accepted"
                       && p.SuggestedBillingInstruction =="CANCEL")
                        && new int[]
                              {
                             RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                             RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                             RunClassificationStatusIds.FINALRUNCOMPLETEDID
                              }.Contains(calc.CalculatorRunClassificationId)
                        select p.ProducerId).AsEnumerable();
            return test;
        }
    }
}