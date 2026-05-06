using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.CancelledProducers
{
    public interface ICalcResultCancelledProducersBuilder
    {
        Task<CalcResultCancelledProducersResponse> ConstructAsync(
            List<MaterialDetail> materialDetails,
            CalcResultsRequestDto resultsRequestDto
        );
    }

    public class CalcResultCancelledProducersBuilder : ICalcResultCancelledProducersBuilder
    {
        private readonly ApplicationDBContext dbContext;
        private readonly IProducerDetailService producerDetailsService;

        public CalcResultCancelledProducersBuilder(
            ApplicationDBContext dbContext,
            IProducerDetailService producerDetailsService
        )
        {
            this.dbContext = dbContext;
            this.producerDetailsService = producerDetailsService;
        }

        public async Task<CalcResultCancelledProducersResponse> ConstructAsync(List<MaterialDetail> materialDetails, CalcResultsRequestDto resultsRequestDto)
        {
            var producers = await GetCancelledProducers(materialDetails, resultsRequestDto.RelativeYear, resultsRequestDto.RunId, resultsRequestDto.IsBillingFile);

            return new CalcResultCancelledProducersResponse
            {
                TitleHeader = CommonConstants.CancelledProducers,
                CancelledProducers = producers
            };
        }

        public async Task<IEnumerable<CalcResultCancelledProducersDto>> GetCancelledProducers(List<MaterialDetail> materialDetails, RelativeYear relativeYear, int runId, bool isBilling)
        {
            IEnumerable<int> allProducerIds = await GetAllProducerIds(relativeYear);

            var producerIdsForCurrentRun = await producerDetailsService.GetProducers(runId);

            var missingProducersIdsInCurrentRun = allProducerIds.Where(t => !producerIdsForCurrentRun.Any(k => k == t)).ToImmutableHashSet();
            var missingProducersInCurrentRun = await producerDetailsService.GetProducerDetails(relativeYear, missingProducersIdsInCurrentRun);

            // populate cancelled producers
            var calcResultCancelledProducers = new List<CalcResultCancelledProducersDto>();
            List<ProducerInvoicedDto> filteredMissingProducers;

            if (isBilling)
            {
                var acceptedCancelledProducersForThisRun = await GetAcceptedCancelledProducersForThisRun(runId);
                filteredMissingProducers = missingProducersInCurrentRun
                    .Where(t => acceptedCancelledProducersForThisRun.Exists(k => k == t.InvoicedTonnage?.ProducerId))
                    .ToList();
            }
            else
            {
                var acceptedCancelledProducersForPreviousRuns = await GetAcceptedCancelledProducers(relativeYear);
                filteredMissingProducers = missingProducersInCurrentRun.Where(t => !acceptedCancelledProducersForPreviousRuns
                .Exists(k => k == t.InvoicedTonnage?.ProducerId)).ToList();
            }


            var distinctMissingProducerIds = filteredMissingProducers.OrderByDescending(t => t.CalculatorRunId).DistinctBy(t => t.InvoicedTonnage?.ProducerId).
            Select(t => t.InvoicedTonnage?.ProducerId).ToList();

            var producerDetails = await GetProducerDetails(distinctMissingProducerIds);

            foreach (var missingProducerId in distinctMissingProducerIds)
            {
                var producerId = missingProducerId is null ? 0 : missingProducerId;

                calcResultCancelledProducers.Add(new CalcResultCancelledProducersDto
                {
                    ProducerId = (int)producerId,
                    ProducerOrSubsidiaryNameValue = producerDetails.FirstOrDefault(t => t.ProducerId == producerId)?.ProducerName,
                    TradingNameValue = producerDetails.FirstOrDefault(t => t.ProducerId == producerId)?.TradingName,

                    LastTonnage = new LastTonnage
                    {
                        AluminiumValue      = GetInvoicedTonnageForMaterials(filteredMissingProducers, materialDetails.First(m => m.Name == MaterialNames.Aluminium     ).Id, producerId),
                        FibreCompositeValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, materialDetails.First(m => m.Name == MaterialNames.FibreComposite).Id, producerId),
                        GlassValue          = GetInvoicedTonnageForMaterials(filteredMissingProducers, materialDetails.First(m => m.Name == MaterialNames.Glass         ).Id, producerId),
                        PaperOrCardValue    = GetInvoicedTonnageForMaterials(filteredMissingProducers, materialDetails.First(m => m.Name == MaterialNames.PaperOrCard   ).Id, producerId),
                        PlasticValue        = GetInvoicedTonnageForMaterials(filteredMissingProducers, materialDetails.First(m => m.Name == MaterialNames.Plastic       ).Id, producerId),
                        WoodValue           = GetInvoicedTonnageForMaterials(filteredMissingProducers, materialDetails.First(m => m.Name == MaterialNames.Wood          ).Id, producerId),
                        SteelValue          = GetInvoicedTonnageForMaterials(filteredMissingProducers, materialDetails.First(m => m.Name == MaterialNames.Steel         ).Id, producerId),
                        OtherMaterialsValue = GetInvoicedTonnageForMaterials(filteredMissingProducers, materialDetails.First(m => m.Name == MaterialNames.OtherMaterials).Id, producerId)
                    },
                    LatestInvoice = new LatestInvoice
                    {
                        BillingInstructionIdValue           = filteredMissingProducers.Where(t => t.InvoiceInstruction?.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(t => t.InvoiceInstruction?.BillingInstructionId).FirstOrDefault(),
                        RunNameValue                        = filteredMissingProducers.Where(t => t.InvoiceInstruction?.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(t => t.CalculatorName).FirstOrDefault(),
                        RunNumberValue                      = filteredMissingProducers.Where(t => t.InvoiceInstruction?.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(t => t.CalculatorRunId).FirstOrDefault().ToString(),
                        CurrentYearInvoicedTotalToDateValue = filteredMissingProducers.Where(t => t.InvoiceInstruction?.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(t => t.InvoiceInstruction?.CurrentYearInvoicedTotalAfterThisRun).FirstOrDefault(),
                    }
                });
            }

            return calcResultCancelledProducers;
        }

        private async Task<IEnumerable<int>> GetAllProducerIds(RelativeYear relativeYear)
        {
            return await (
                from prfb in dbContext.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                join cr in dbContext.CalculatorRuns.AsNoTracking()
                on prfb.CalculatorRunId equals cr.Id
                where
                   new[]
                   {
                      RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                      RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                      RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                      RunClassificationStatusIds.FINALRUNCOMPLETEDID
                   }.Contains(cr.CalculatorRunClassificationId) && cr.RelativeYearValue == relativeYear.Value
                   && prfb.BillingInstructionAcceptReject == CommonConstants.Accepted
                select prfb.ProducerId
            ).ToListAsync();
        }

        private static decimal? GetInvoicedTonnageForMaterials(List<ProducerInvoicedDto> cancelledProducersWithData, int materialId, int? producerId)
        {
            return cancelledProducersWithData.Where(t => t.InvoicedTonnage?.MaterialId == materialId && t.InvoicedTonnage.ProducerId == producerId).OrderByDescending(t => t.CalculatorRunId).Select(k => k.InvoicedTonnage?.InvoicedNetTonnage).FirstOrDefault();
        }

        private async Task<List<int>> GetAcceptedCancelledProducers(RelativeYear relativeYear)
        {
            return await (
                from calc in dbContext.CalculatorRuns.AsNoTracking()
                join p in dbContext.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                    on calc.Id equals p.CalculatorRunId
                where calc.RelativeYearValue == relativeYear.Value
                    && p.BillingInstructionAcceptReject != null
                    && p.BillingInstructionAcceptReject == CommonConstants.Accepted
                    && p.SuggestedBillingInstruction == CommonConstants.CancelStatus
                    && new[]
                    {
                        RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                        RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                        RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                        RunClassificationStatusIds.FINALRUNCOMPLETEDID
                    }.Contains(calc.CalculatorRunClassificationId)
                select p.ProducerId
            ).ToListAsync();
        }

        private async Task<List<int>> GetAcceptedCancelledProducersForThisRun(int runId)
        {
            return await (
                from p in dbContext.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                where p.CalculatorRunId == runId
                    && p.BillingInstructionAcceptReject == CommonConstants.Accepted
                    && p.SuggestedBillingInstruction == CommonConstants.CancelStatus
                select p.ProducerId
            ).ToListAsync();
        }


        private async Task<IEnumerable<ProducerDetail>> GetProducerDetails(IEnumerable<int?> producerIds)
        {
            return await dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking()
                .OrderByDescending(t => t.CalculatorRunOrganisationDataMasterId)
                .Where(t => producerIds.Contains(t.OrganisationId) && string.IsNullOrEmpty(t.SubsidiaryId))
                .Select(t => new ProducerDetail { ProducerId= t.OrganisationId, ProducerName= t.OrganisationName, TradingName = t.TradingName })
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
