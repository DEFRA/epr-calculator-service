namespace EPR.Calculator.Service.Function.Builder.CancelledProducers
{
    using System;
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
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultCancelledProducersBuilder : ICalcResultCancelledProducersBuilder
    {
        private readonly ApplicationDBContext context;
        private readonly IMaterialService materialService;
        private List<MaterialDetail> materials;
        private readonly IProducerDetailService producerDetailsService;

        public CalcResultCancelledProducersBuilder(ApplicationDBContext context,
            IMaterialService materialService,
            IProducerDetailService producerDetailsService)
        {
            this.context = context;
            this.materialService = materialService;
            this.materials = new List<MaterialDetail>();
            this.producerDetailsService = producerDetailsService;
        }

        public async Task<CalcResultCancelledProducersResponse> ConstructAsync(CalcResultsRequestDto resultsRequestDto, string financialYear)
        {

            this.materials = this.materialService.GetMaterials().Result;

            return await Task.Run(() =>
            {
                var producers = new List<CalcResultCancelledProducersDto>();
                producers.AddRange(GetCancelledProducers(financialYear, resultsRequestDto.RunId, resultsRequestDto.IsBillingFile).Result);

                var response = new CalcResultCancelledProducersResponse
                {
                    TitleHeader = CommonConstants.CancelledProducers,
                    CancelledProducers = producers
                };

                return response;
            });
        }

        public async Task<IEnumerable<CalcResultCancelledProducersDto>> GetCancelledProducers(string financialYear, int runId, bool isBilling)
        {
            IEnumerable<int> allProducerIds = await GetAllProducerIds(financialYear);

            var producersForCurrentRun = this.producerDetailsService.GetProducers(runId);

            var missingProducersIdsInCurrentRun = allProducerIds.Where(t => !producersForCurrentRun.Any(k => k.ProducerId == t));
            var missingProducersInCurrentRun = await this.producerDetailsService.GetLatestProducerDetailsForThisFinancialYear(financialYear, missingProducersIdsInCurrentRun);

            // populate cancelled producers
            var calcResultCancelledProducers = new List<CalcResultCancelledProducersDto>();
            var filteredMissingProducers = new List<ProducerInvoicedDto>();

            if (isBilling)
            {
                var acceptedCancelledProducersForThisRun = GetAcceptedCancelledProducersForThisRun(runId).ToList();
                filteredMissingProducers = missingProducersInCurrentRun.Where(t => acceptedCancelledProducersForThisRun.
                Exists(k => k == t.InvoicedTonnage?.ProducerId)).ToList();

            }
            else
            {
                var acceptedCancelledProducersForPreviousRuns = GetAcceptedCancelledProducers(financialYear).ToList();
                filteredMissingProducers = missingProducersInCurrentRun.Where(t => !acceptedCancelledProducersForPreviousRuns
                .Exists(k => k == t.InvoicedTonnage?.ProducerId)).ToList();
            }


            var distinctMissingProducerIds = filteredMissingProducers.OrderByDescending(t => t.CalculatorRunId).DistinctBy(t => t.InvoicedTonnage?.ProducerId).
            Select(t => t.InvoicedTonnage?.ProducerId).ToList();

            var producerDetails = await GetProducerDetails(distinctMissingProducerIds);

            foreach (var missingProducerId in distinctMissingProducerIds)
            {
                var producerId = missingProducerId is null ? 0 : missingProducerId;

                calcResultCancelledProducers.Add(new CalcResultCancelledProducersDto()
                {
                    ProducerId = (int)producerId,
                    ProducerOrSubsidiaryNameValue = producerDetails.FirstOrDefault(t => t.ProducerId == producerId)?.ProducerName,
                    TradingNameValue = producerDetails.FirstOrDefault(t => t.ProducerId == producerId)?.TradingName,

                    LastTonnage = new LastTonnage()
                    {
                        AluminiumValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, GetMaterialId(MaterialNames.Aluminium), producerId),
                        FibreCompositeValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, GetMaterialId(MaterialNames.FibreComposite), producerId),
                        GlassValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, GetMaterialId(MaterialNames.Glass), producerId),
                        PaperOrCardValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, GetMaterialId(MaterialNames.PaperOrCard), producerId),
                        PlasticValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, GetMaterialId(MaterialNames.Plastic), producerId),
                        WoodValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, GetMaterialId(MaterialNames.Wood), producerId),
                        SteelValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, GetMaterialId(MaterialNames.Steel), producerId),
                        OtherMaterialsValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, GetMaterialId(MaterialNames.OtherMaterials), producerId)
                    },
                    LatestInvoice = new LatestInvoice
                    {
                        BillingInstructionIdValue = filteredMissingProducers.Where(t => t.InvoiceInstruction?.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(t => t.InvoiceInstruction?.BillingInstructionId).FirstOrDefault(),
                        RunNameValue = filteredMissingProducers.Where(t => t.InvoiceInstruction?.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(t => t.CalculatorName).FirstOrDefault(),
                        RunNumberValue = filteredMissingProducers.Where(t => t.InvoiceInstruction?.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(t => t.CalculatorRunId).FirstOrDefault().ToString(),
                        CurrentYearInvoicedTotalToDateValue = filteredMissingProducers.Where(t => t.InvoiceInstruction?.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(t => t.InvoiceInstruction?.CurrentYearInvoicedTotalAfterThisRun).FirstOrDefault(),
                    }
                });
            }

            return calcResultCancelledProducers;
        }

        private async Task<IEnumerable<int>> GetAllProducerIds(string financialYear)
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
        
        private static decimal? GetInvoicedTonnageForMaterials(List<ProducerInvoicedDto> cancelledProducersWithData, int materialId, int? producerId)
        {
            return cancelledProducersWithData.Where(t => t.InvoicedTonnage?.MaterialId == materialId && t.InvoicedTonnage.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(k => k.InvoicedTonnage?.InvoicedNetTonnage).FirstOrDefault();
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
                                                && p.SuggestedBillingInstruction == CommonConstants.CancelStatus)
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

        private IEnumerable<int> GetAcceptedCancelledProducersForThisRun(int runId)
        {
            var cancelledAcceptedProducers = (from p in context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                                              where (p.CalculatorRunId == runId && p.BillingInstructionAcceptReject == CommonConstants.Accepted
                                              && p.SuggestedBillingInstruction == CommonConstants.CancelStatus)
                                              select p.ProducerId).AsEnumerable();
            return cancelledAcceptedProducers;
        }


        private async Task<IEnumerable<ProducerDetail>> GetProducerDetails(IEnumerable<int?> producerIds)
        {
            return await context.CalculatorRunOrganisationDataDetails
                .AsNoTracking()
                .OrderByDescending(t => t.CalculatorRunOrganisationDataMasterId)
                .Where(t => producerIds.Contains(t.OrganisationId.GetValueOrDefault()) && string.IsNullOrEmpty(t.SubsidaryId))
                .Select(t => new ProducerDetail { ProducerId= t.OrganisationId.GetValueOrDefault(), ProducerName= t.OrganisationName, TradingName = t.TradingName })
                .ToListAsync();
        }

        private sealed record ProducerDetail
        {
            public int ProducerId { get; set; }
            public required string ProducerName
            {
                get; set;
            }

            public string? TradingName { get; set; }
        }
    }
}