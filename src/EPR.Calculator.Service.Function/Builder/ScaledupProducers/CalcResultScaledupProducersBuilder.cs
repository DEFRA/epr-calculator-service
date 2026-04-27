using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    public interface ICalcResultScaledupProducersBuilder
    {
        Task<(List<ProducerDetail>, CalcResultScaledupProducers)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerDetail> producerDetails);
    }

    public class CalcResultScaledupProducersBuilder : ICalcResultScaledupProducersBuilder
    {
        private const decimal NormalScaleup = 1.0M;
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 10;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;

        private readonly ApplicationDBContext dbContext;

        public CalcResultScaledupProducersBuilder(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public static CalcResultScaledupProducer GetOverallTotalRow(
            IEnumerable<CalcResultScaledupProducer> orderedRunProducerMaterialDetails,
            IEnumerable<MaterialDetail> materials
        )
        {
            var overallTotalRow = new CalcResultScaledupProducer
            {
                IsTotalRow = true,
                ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>(),
            };

            var allMaterialDict = orderedRunProducerMaterialDetails.Where(x => !x.IsSubtotalRow).Select(x => x.ScaledupProducerTonnageByMaterial);
            foreach (var material in materials)
            {
                var totalRow = new CalcResultScaledupProducerTonnage();
                var materialValues = allMaterialDict.Where(x => x.ContainsKey(material.Code)).Select(x => x[material.Code]).ToList();
                totalRow.ReportedHouseholdPackagingWasteTonnage = materialValues.Sum(x => x.ReportedHouseholdPackagingWasteTonnage);
                totalRow.ReportedPublicBinTonnage = materialValues.Sum(x => x.ReportedPublicBinTonnage);
                if (material.Code == MaterialCodes.Glass)
                {
                    totalRow.HouseholdDrinksContainersTonnageGlass = materialValues.Sum(x => x.HouseholdDrinksContainersTonnageGlass);
                }

                totalRow.TotalReportedTonnage = materialValues.Sum(x => x.TotalReportedTonnage);
                totalRow.ReportedSelfManagedConsumerWasteTonnage = materialValues.Sum(x => x.ReportedSelfManagedConsumerWasteTonnage);
                totalRow.NetReportedTonnage = materialValues.Sum(x => x.NetReportedTonnage);
                totalRow.ScaledupReportedHouseholdPackagingWasteTonnage = materialValues.Sum(x => x.ScaledupReportedHouseholdPackagingWasteTonnage);
                totalRow.ScaledupReportedPublicBinTonnage = materialValues.Sum(x => x.ScaledupReportedPublicBinTonnage);
                if (material.Code == MaterialCodes.Glass)
                {
                    totalRow.ScaledupHouseholdDrinksContainersTonnageGlass = materialValues.Sum(x => x.ScaledupHouseholdDrinksContainersTonnageGlass);
                }

                totalRow.ScaledupTotalReportedTonnage = materialValues.Sum(x => x.ScaledupTotalReportedTonnage);
                totalRow.ScaledupReportedSelfManagedConsumerWasteTonnage = materialValues.Sum(x => x.ScaledupReportedSelfManagedConsumerWasteTonnage);
                totalRow.ScaledupNetReportedTonnage = materialValues.Sum(x => x.ScaledupNetReportedTonnage);

                overallTotalRow.ScaledupProducerTonnageByMaterial.Add(material.Name, totalRow);
            }

            return overallTotalRow;
        }

        public static void CalculateScaledupTonnage(
            IEnumerable<CalcResultScaledupProducer> runScaledUpProducers,
            List<ProducerDetail> producerDetails,
            IEnumerable<MaterialDetail> materials
        )
        {
            var byProducerAndSubsidiary = producerDetails
                .GroupBy(pd => (pd.ProducerId, pd.SubsidiaryId))
                .ToDictionary(g => g.Key, g => g.ToList());

            var byProducer = producerDetails
                .GroupBy(pd => pd.ProducerId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var item in runScaledUpProducers)
            {
                var scaledProducerDetails = item.IsSubtotalRow
                    ? byProducer.GetValueOrDefault(item.ProducerId, [])
                    : byProducerAndSubsidiary.GetValueOrDefault((item.ProducerId, item.SubsidiaryId), []);

                var pomData = scaledProducerDetails
                    .SelectMany(pd => pd.ProducerReportedMaterials)
                    .Where(rm => rm.SubmissionPeriod == item.SubmissionPeriodCode)
                    .ToList();

                item.ScaledupProducerTonnageByMaterial = GetTonnages(pomData, materials, item.ScaleupFactor);
            }
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
                var first = pair.ToList()[0];

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

        /// <inheritdoc/>
        public async Task<(List<ProducerDetail>, CalcResultScaledupProducers)> ConstructAsync(
            CalcResultsRequestDto resultsRequestDto,
            List<ProducerDetail> producerDetails
        )
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await dbContext.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var scaledupOrganisations = await GetScaledUpOrganisationsAsync(runId);

            if (!scaledupOrganisations.Any())
            {
                var emptyResult = new CalcResultScaledupProducers { ScaledupProducers = [] };
                SetHeaders(emptyResult, materials);
                return (producerDetails, emptyResult);
            }

            var organisationIds = scaledupOrganisations.Select(so => so.OrganisationId).ToList();
            var scaledUpProducers = await GetScaledUpProducersAsync(runId, organisationIds);
            AddExtraRows(scaledUpProducers, scaledupOrganisations);

            // ScaleupFactor is period-based, so it is identical across all subsidiaries of the same
            // ProducerId (SubsidiaryId not needed in lookup).
            var scaleupFactorByProducer = scaledUpProducers
                .Where(s => !s.IsSubtotalRow && !s.IsTotalRow)
                .GroupBy(s => s.ProducerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(s => s.SubmissionPeriodCode ?? "N/A")
                          .ToDictionary(sg => sg.Key, sg => sg.First().ScaleupFactor)
                );

            var displayRowLookup = scaledUpProducers
                .Where(s => !s.IsSubtotalRow && !s.IsTotalRow)
                .GroupBy(s => (s.ProducerId, s.SubsidiaryId))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(s => s.SubmissionPeriodCode ?? "N/A", s => s)
                );

            var subtotalLookup = scaledUpProducers
                .Where(s => s.IsSubtotalRow)
                .ToDictionary(s => (s.ProducerId, s.SubmissionPeriodCode ?? "N/A"));

            // Aggregates original (pre-scale) POM data per (ProducerId, Period) for subtotal rows
            var subtotalAccumulator = new Dictionary<(int, string), List<ProducerReportedMaterial>>();
            var updatedProducers = new List<ProducerDetail>(producerDetails.Count);

            foreach (var pd in producerDetails)
            {
                if (!scaleupFactorByProducer.TryGetValue(pd.ProducerId, out var factorByPeriod))
                {
                    updatedProducers.Add(pd);
                    continue;
                }

                // Populate this producer's display rows using original tonnages (before scaling)
                if (displayRowLookup.TryGetValue((pd.ProducerId, pd.SubsidiaryId), out var periodToRow))
                {
                    foreach (var (period, row) in periodToRow)
                    {
                        var pomData = pd.ProducerReportedMaterials
                            .Where(rm => rm.SubmissionPeriod == period)
                            .ToList();
                        row.ScaledupProducerTonnageByMaterial = GetTonnages(pomData, materials, row.ScaleupFactor);

                        var key = (pd.ProducerId, period);
                        if (!subtotalAccumulator.ContainsKey(key))
                            subtotalAccumulator[key] = [];
                        subtotalAccumulator[key].AddRange(pomData);
                    }
                }

                updatedProducers.Add(CalcResultPartialObligationBuilder.UpdateReportedMaterials(
                    pd,
                    reportedMaterials => reportedMaterials.Select(rm =>
                        factorByPeriod.TryGetValue(rm.SubmissionPeriod, out var factor) ? scale(rm, factor) : rm
                    ).ToList()
                ));
            }

            foreach (var (key, subtotalRow) in subtotalLookup)
            {
                if (subtotalAccumulator.TryGetValue(key, out var pomData))
                    subtotalRow.ScaledupProducerTonnageByMaterial = GetTonnages(pomData, materials, subtotalRow.ScaleupFactor);
            }

            var orderedRows = scaledUpProducers
                .OrderBy(p => p.ProducerId)
                .ThenBy(p => p.Level)
                .ThenBy(p => p.SubsidiaryId)
                .ThenBy(p => p.SubmissionPeriodCode)
                .ToList();

            orderedRows.Add(GetOverallTotalRow(orderedRows, materials));

            var scaledupProducersSummary = new CalcResultScaledupProducers { ScaledupProducers = orderedRows };
            SetHeaders(scaledupProducersSummary, materials);
            return (updatedProducers, scaledupProducersSummary);
        }

        private ProducerReportedMaterial scale(ProducerReportedMaterial reportedMaterial, decimal scaleupFactor)
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

        public static Dictionary<string, CalcResultScaledupProducerTonnage> GetTonnages(
            List<ProducerReportedMaterial> pomData,
            IEnumerable<MaterialDetail> materials,
            decimal scaleUpFactor
        )
        {
            var scaledupProducerTonnages = new Dictionary<string, CalcResultScaledupProducerTonnage>();

            foreach (var material in materials)
            {
                var scaledupProducerTonnage = new CalcResultScaledupProducerTonnage();

                var materialPomData =
                   pomData.Where(rm => rm.MaterialId == material.Id);

                scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage = materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.Household)
                    .Sum(pom => pom.PackagingTonnage);

                scaledupProducerTonnage.ReportedPublicBinTonnage = materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.PublicBin)
                    .Sum(pom => pom.PackagingTonnage);

                var hdc = materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.HouseholdDrinksContainers)
                    .Sum(pom => pom.PackagingTonnage);

                if (material.Code == MaterialCodes.Glass)
                {
                    scaledupProducerTonnage.HouseholdDrinksContainersTonnageGlass = hdc;
                    scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                                            scaledupProducerTonnage.ReportedPublicBinTonnage + scaledupProducerTonnage.HouseholdDrinksContainersTonnageGlass;
                }
                else
                {
                    scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                    scaledupProducerTonnage.ReportedPublicBinTonnage;
                }

                scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage = materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.ConsumerWaste)
                    .Sum(pom => pom.PackagingTonnage);

                scaledupProducerTonnage.NetReportedTonnage = scaledupProducerTonnage.TotalReportedTonnage - scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage;
                scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage = Math.Round(scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage * scaleUpFactor, 3);
                scaledupProducerTonnage.ScaledupReportedPublicBinTonnage = Math.Round(scaledupProducerTonnage.ReportedPublicBinTonnage * scaleUpFactor, 3);
                if (material.Code == MaterialCodes.Glass)
                {
                    scaledupProducerTonnage.ScaledupHouseholdDrinksContainersTonnageGlass = scaledupProducerTonnage.HouseholdDrinksContainersTonnageGlass * scaleUpFactor;
                }

                scaledupProducerTonnage.ScaledupTotalReportedTonnage = Math.Round(scaledupProducerTonnage.TotalReportedTonnage * scaleUpFactor, 3);
                scaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = Math.Round(scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage * scaleUpFactor, 3);
                scaledupProducerTonnage.ScaledupNetReportedTonnage = Math.Round(scaledupProducerTonnage.NetReportedTonnage * scaleUpFactor, 3);
                scaledupProducerTonnages.Add(material.Code, scaledupProducerTonnage);
            }

            return scaledupProducerTonnages;
        }

        public static void SetHeaders(CalcResultScaledupProducers producers, IEnumerable<MaterialDetail> materials)
        {
            producers.TitleHeader = new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.ScaledupProducers,
                ColumnIndex = 1,
            };

            producers.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials);

            producers.ColumnHeaders = GetColumnHeaders(materials);
        }

        public static List<CalcResultScaledupProducerHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<CalcResultScaledupProducerHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.EachSubmissionForTheYear,
                ColumnIndex = 1,
            });

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex,
                });

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex + 2
                    : columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            return materialsBreakdownHeaders;
        }

        public static List<CalcResultScaledupProducerHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = new List<CalcResultScaledupProducerHeader>();

            columnHeaders.AddRange([
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubsidiaryId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerOrSubsidiaryName },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TradingName },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.Level },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubmissionPeriodCode },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInSubmissionPeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInWholePeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaleupFactor }
            ]);

            foreach (var material in materials)
            {
                var columnHeadersList = new List<CalcResultScaledupProducerHeader>
                {
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.PublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TotalTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.NetTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupPublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupTotalTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupSelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupNetTonnage },
                };

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeadersList.Insert(2, new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdDrinksContainersTonnageGlass });
                    columnHeadersList.Insert(8, new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdDrinksContainersTonnageGlass });
                }

                columnHeaders.AddRange(columnHeadersList);
            }

            return columnHeaders;
        }
    }
}
