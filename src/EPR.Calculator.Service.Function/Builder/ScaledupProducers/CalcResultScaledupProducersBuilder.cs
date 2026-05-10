using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    public interface ICalcResultScaledupProducersBuilder
    {
        Task<(List<ProducerDetail>, CalcResultScaledupProducers)> ConstructAsync(
            List<MaterialDetail> materialDetails,
            List<ProducerDetail> producerDetails,
            CalcResultsRequestDto resultsRequestDto
        );
    }

    public class CalcResultScaledupProducersBuilder : ICalcResultScaledupProducersBuilder
    {
        private const decimal NormalScaleup = 1.0M;

        private readonly ApplicationDBContext dbContext;

        public CalcResultScaledupProducersBuilder(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public static void AddExtraRows(List<CalcResultScaledupProducer> runScaledUpProducers, IEnumerable<Organisation> scaledupOrganisations)
        {
            var level2Rows = runScaledUpProducers
                .Where(x => string.IsNullOrEmpty(x.SubsidiaryId))
                .GroupBy(x => new { x.ProducerId, x.SubsidiaryId }).ToList();

            foreach (var row in level2Rows)
            {
                if (runScaledUpProducers.Exists(x => x.ProducerId == row.Key.ProducerId && x.SubsidiaryId != null)
                    &&
                    row.Any())
                {
                    var levelRows = runScaledUpProducers.Where(x => x.ProducerId == row.Key.ProducerId && string.IsNullOrEmpty(x.SubsidiaryId));
                    foreach (var level2Row in levelRows)
                    {
                        level2Row.Level = CommonConstants.LevelTwo.ToString();
                    }
                }
            }

            var groupByResult = runScaledUpProducers
                .Where(x => x.SubsidiaryId != null)
                .GroupBy(x => new { x.ProducerId, x.SubmissionPeriodCode })
                .ToList();

            foreach (var pair in groupByResult)
            {
                var first = pair.First();

                var parentProducer = scaledupOrganisations.FirstOrDefault(so => so.OrganisationId == pair.Key.ProducerId);

                if (parentProducer != null)
                {
                    var extraRow = new CalcResultScaledupProducer
                    {
                        ProducerId = pair.Key.ProducerId,
                        SubsidiaryId = string.Empty,
                        ProducerName = parentProducer.OrganisationName,
                        TradingName = parentProducer.TradingName,
                        ScaleupFactor = first.ScaleupFactor,
                        SubmissionPeriodCode = pair.Key.SubmissionPeriodCode,
                        DaysInSubmissionPeriod = first.DaysInSubmissionPeriod,
                        DaysInWholePeriod = first.DaysInWholePeriod,
                        Level = CommonConstants.LevelOne.ToString(),
                        IsSubtotalRow = true,
                    };

                    runScaledUpProducers.Add(extraRow);
                }
            }
        }

        [SuppressMessage(
            "Critical Code Smell",
            "S3776:Cognitive Complexity of methods should not be too high",
            Justification = "Temporaraly suppress - will refactor later.")]
        public async Task<(List<ProducerDetail>, CalcResultScaledupProducers)> ConstructAsync(
            List<MaterialDetail> materialDetails,
            List<ProducerDetail> producerDetails,
            CalcResultsRequestDto resultsRequestDto
        )
        {
            var runId = resultsRequestDto.RunId;

            var scaledupOrganisations = await GetScaledUpOrganisationsAsync(runId);

            if (!scaledupOrganisations.Any())
            {
                var emptyResult = new CalcResultScaledupProducers { Materials = materialDetails.ToImmutableList(), ScaledupProducers = [] };
                return (producerDetails, emptyResult);
            }

            var organisationIds = scaledupOrganisations.Select(so => so.OrganisationId).ToList();
            var scaledUpProducers = await GetScaledUpProducersAsync(runId, organisationIds);
            AddExtraRows(scaledUpProducers, scaledupOrganisations);

            // ScaleupFactor is period-based, so it is identical across all subsidiaries of the same
            // ProducerId (SubsidiaryId not needed in lookup).
            var scaleupFactorByProducer = scaledUpProducers
                .Where(s => !s.IsSubtotalRow)
                .GroupBy(s => s.ProducerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(s => s.SubmissionPeriodCode ?? "N/A")
                          .ToDictionary(sg => sg.Key, sg => sg.First().ScaleupFactor)
                );

            var displayRowLookup = scaledUpProducers
                .Where(s => !s.IsSubtotalRow)
                .GroupBy(s => (s.ProducerId, s.SubsidiaryId))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(s => s.SubmissionPeriodCode ?? "N/A", s => s)
                );

            var subtotalLookup = scaledUpProducers
                .Where(s => s.IsSubtotalRow)
                .ToDictionary(s => (s.ProducerId, s.SubmissionPeriodCode ?? "N/A"));

            // Aggregates original (pre-scale) POM data per (ProducerId, Period) for subtotal rows
            var subtotalAccumulator = new Dictionary<(int, string), List<ScaledupPomEntry>>();
            var updatedProducers = new List<ProducerDetail>(producerDetails.Count);

            foreach (var pd in producerDetails)
            {
                if (!scaleupFactorByProducer.TryGetValue(pd.ProducerId, out var factorByPeriod))
                {
                    updatedProducers.Add(pd);
                    continue;
                }

                // Scale each material once: the scaled result feeds both the pipeline and the display entry
                var scaledWithEntries = pd.ProducerReportedMaterials
                    .Select(rm =>
                    {
                        var scaledRm = factorByPeriod.TryGetValue(rm.SubmissionPeriod, out var factor) ? Scale(rm, factor) : rm;
                        return (scaledRm, entry: new ScaledupPomEntry(rm.MaterialId, rm.PackagingType, rm.PackagingTonnage, scaledRm.PackagingTonnage));
                    })
                    .ToList();

                if (displayRowLookup.TryGetValue((pd.ProducerId, pd.SubsidiaryId), out var periodToRow))
                {
                    foreach (var (period, row) in periodToRow)
                    {
                        var entries = scaledWithEntries
                            .Where(x => x.scaledRm.SubmissionPeriod == period)
                            .Select(x => x.entry)
                            .ToList();
                        row.PomData = entries;

                        var key = (pd.ProducerId, period);
                        if (!subtotalAccumulator.ContainsKey(key))
                            subtotalAccumulator[key] = [];
                        subtotalAccumulator[key].AddRange(entries);
                    }
                }

                updatedProducers.Add(CalcResultPartialObligationBuilder.UpdateReportedMaterials(
                    pd,
                    _ => scaledWithEntries.Select(x => x.scaledRm).ToList()
                ));
            }

            foreach (var (key, subtotalRow) in subtotalLookup)
            {
                if (subtotalAccumulator.TryGetValue(key, out var pomData))
                    subtotalRow.PomData = pomData;
            }

            var orderedRows = scaledUpProducers
                .OrderBy(p => p.ProducerId)
                .ThenBy(p => p.Level)
                .ThenBy(p => p.SubsidiaryId)
                .ThenBy(p => p.SubmissionPeriodCode)
                .ToList();

            var scaledupProducersSummary = new CalcResultScaledupProducers {  Materials = materialDetails.ToImmutableList(), ScaledupProducers = orderedRows.ToImmutableList() };
            return (updatedProducers, scaledupProducersSummary);
        }

        private ProducerReportedMaterial Scale(ProducerReportedMaterial reportedMaterial, decimal scaleupFactor)
        {
            // only scale total - Ram doesn't apply to 2025 relative year (2024 pom)
            var tonnage  = Math.Round(scaleupFactor * reportedMaterial.PackagingTonnage, 3);
            return new ProducerReportedMaterial
            {
                Id = reportedMaterial.Id,
                MaterialId = reportedMaterial.MaterialId,
                ProducerDetailId = reportedMaterial.ProducerDetailId,
                PackagingType = reportedMaterial.PackagingType,
                PackagingTonnage = tonnage,
                SubmissionPeriod = reportedMaterial.SubmissionPeriod,
	            ProducerDetail = reportedMaterial.ProducerDetail,
	            Material = reportedMaterial.Material
            };
        }

        public async Task<List<CalcResultScaledupProducer>> GetScaledUpProducersAsync(int runId, IEnumerable<int> organisationIds)
        {
            return await (
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join crpdd in dbContext.CalculatorRunPomDataDetails.AsNoTracking() on run.CalculatorRunPomDataMasterId equals crpdd.CalculatorRunPomDataMasterId
                join spl in dbContext.SubmissionPeriodLookup.AsNoTracking() on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                join pd in dbContext.ProducerDetail.AsNoTracking() on crpdd.OrganisationId equals pd.ProducerId
                join org in dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking()
                  on new { pd.ProducerId, pd.SubsidiaryId, crpdd.SubmitterId }
                    equals new { ProducerId = org.OrganisationId, org.SubsidiaryId, org.SubmitterId }
                where run.Id == runId && organisationIds.Contains(crpdd.OrganisationId.GetValueOrDefault())
                  && pd.CalculatorRunId == runId && org.ObligationStatus == ObligationStates.Obligated
                select new CalcResultScaledupProducer
                {
                    ProducerId = pd.ProducerId,
                    SubsidiaryId = pd.SubsidiaryId!,
                    ProducerName = pd.ProducerName!,
                    TradingName = pd.TradingName!,
                    ScaleupFactor = spl.ScaleupFactor,
                    SubmissionPeriodCode = spl.SubmissionPeriod,
                    DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                    DaysInWholePeriod = spl.DaysInWholePeriod,
                    Level = pd.SubsidiaryId != null ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                }
            ).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<Organisation>> GetScaledUpOrganisationsAsync(int runId)
        {
            var scaleupOrganisationIds =
                await (
                    from run in dbContext.CalculatorRuns.AsNoTracking()
                    join crpdm in dbContext.CalculatorRunPomDataMaster.AsNoTracking() on run.CalculatorRunPomDataMasterId equals crpdm.Id
                    join crpdd in dbContext.CalculatorRunPomDataDetails.AsNoTracking() on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                    join spl in dbContext.SubmissionPeriodLookup.AsNoTracking() on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                    where run.Id == runId && crpdd.OrganisationId != null && spl.ScaleupFactor > NormalScaleup
                    select crpdd.OrganisationId.GetValueOrDefault()
                ).Distinct().ToListAsync() ?? [];

            var filteredCrodds =
                dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking()
                    .Where(x => scaleupOrganisationIds.Contains(x.OrganisationId) && x.SubsidiaryId == null);

            var scaledupOrganisations =
                await (
                    from run in dbContext.CalculatorRuns.AsNoTracking()
                    join crodm in dbContext.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                    join crodd in filteredCrodds on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                    where run.Id == runId
                    select new Organisation
                    {
                        OrganisationId = crodd.OrganisationId,
                        OrganisationName = crodd.OrganisationName,
                        TradingName = crodd.TradingName
                    }
                ).AsNoTracking().Distinct().ToListAsync();

            return scaledupOrganisations;
        }
    }
}
