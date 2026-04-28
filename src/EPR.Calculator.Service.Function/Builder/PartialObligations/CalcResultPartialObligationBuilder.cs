using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Builder.PartialObligations
{
    public record PartialObligationData
    {

    }

    public interface ICalcResultPartialObligationBuilder
    {
        //Task<CalcResultPartialObligations> ConstructAsync(CalcResultsRequestDto resultsRequestDto, IEnumerable<CalcResultScaledupProducer> scaledupProducers);
        Task<(List<ProducerReportedMaterialsForSubmissionPeriod>, CalcResultPartialObligations)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerReportedMaterialsForSubmissionPeriod> producers);
    }

    public class CalcResultPartialObligationBuilder : ICalcResultPartialObligationBuilder
    {

        private readonly ApplicationDBContext context;
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 11;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;

        public CalcResultPartialObligationBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<List<CalcResultPartialObligation>> GetPartialObligations2(int runId)
        {
            return await (
                from run in context.CalculatorRuns.AsNoTracking()
                join crodm in context.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in context.CalculatorRunOrganisationDataDetails.AsNoTracking() on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join pd in context.ProducerDetail.Include(x => x.ProducerReportedMaterials) on crodd.OrganisationId equals pd.ProducerId
                where run.Id == runId && crodd.ObligationStatus == ObligationStates.Obligated && crodd.DaysObligated != null && crodd.SubsidiaryId == pd.SubsidiaryId && pd.CalculatorRunId == runId
                let daysInYear = DateTime.IsLeapYear(crodm.RelativeYear.Value) ? 366 : 365
                let partialAmount = crodd.DaysObligated != null ? (decimal)crodd.DaysObligated! / daysInYear : 1
                select new CalcResultPartialObligation
                {
                    ProducerId = pd.ProducerId,
                    SubsidiaryId = pd.SubsidiaryId,
                    ProducerName = pd.ProducerName,
                    TradingName = pd.TradingName,
                    Level = pd.SubsidiaryId != null ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                    SubmissionYear = crodm.RelativeYear.Value.ToString(),
                    DaysInSubmissionYear = daysInYear,
                    JoiningDate = crodd.JoinerDate,
                    DaysObligated = crodd.DaysObligated,
                    ObligatedPercentage = (partialAmount * 100).ToString("F2") + "%"
                }
            ).ToListAsync();
        }

        private ProducerReportedMaterial scale(ProducerReportedMaterial reportedMaterial, CalcResultPartialObligation partialObligation)
        {
            // TODO scaling
            var p = decimal.Parse(partialObligation.ObligatedPercentage.TrimEnd('%')) / 100;// TODO better way to extract?
            var r = Math.Round(p * reportedMaterial.PackagingTonnageRed ?? 0m, 3);
	        var a = Math.Round(p * reportedMaterial.PackagingTonnageAmber ?? 0m, 3);
	        var g = Math.Round(p * reportedMaterial.PackagingTonnageGreen ?? 0m, 3);
	        var rm = Math.Round(p * reportedMaterial.PackagingTonnageRedMedical ?? 0m, 3);
	        var am = Math.Round(p * reportedMaterial.PackagingTonnageAmberMedical ?? 0m, 3);
	        var gm = Math.Round(p * reportedMaterial.PackagingTonnageGreenMedical ?? 0m, 3);
            return new ProducerReportedMaterial
            {
                Id = reportedMaterial.Id,
                MaterialId = reportedMaterial.MaterialId,
                ProducerDetailId = reportedMaterial.ProducerDetailId,
                PackagingType = reportedMaterial.PackagingType,
                PackagingTonnage =  r + a + g + rm + am + gm,
                PackagingTonnageRed = r,
	            PackagingTonnageAmber = a,
	            PackagingTonnageGreen = g,
	            PackagingTonnageRedMedical = g,
	            PackagingTonnageAmberMedical = am,
	            PackagingTonnageGreenMedical = gm,
                SubmissionPeriod = reportedMaterial.SubmissionPeriod,
	            ProducerDetail = reportedMaterial.ProducerDetail,
	            Material = reportedMaterial.Material
            };
        }

        public async Task<List<ProducerReportedMaterialsForSubmissionPeriod>> updateProducerData(CalcResultsRequestDto resultsRequestDto, List<ProducerReportedMaterialsForSubmissionPeriod> producers)
        {
            // TODO dictionary
            var partialObligations = await GetPartialObligations2(resultsRequestDto.RunId);
            return producers.Select(p =>
            {
                var partialObligation = partialObligations.Where(q => q.ProducerId == p.ProducerId && q.SubsidiaryId == p.SubsidiaryId).FirstOrDefault();
                Console.WriteLine($">> partialObligation for {p.ProducerId} {p.SubsidiaryId} {JsonConvert.SerializeObject(partialObligation, Formatting.Indented)}");
                if (partialObligation != null)
                {
                    var a = new ProducerReportedMaterialsForSubmissionPeriod(
                         producerId : p.ProducerId,
                         subsidiaryId : p.SubsidiaryId,
                         submissionPeriod : p.SubmissionPeriod,
                         reportedMaterials : p.ReportedMaterials.Select(rm => scale(rm, partialObligation!)).ToList()
                    );
                    Console.WriteLine($">> returning for {p.ProducerId} {p.SubsidiaryId} *{JsonConvert.SerializeObject(partialObligation, Formatting.Indented)}");
                    return a;
                }
                else
                {
                    return p;
                }
            }).ToList();
        }

        public async Task<(List<ProducerReportedMaterialsForSubmissionPeriod>, CalcResultPartialObligations)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerReportedMaterialsForSubmissionPeriod> producers)
        {
            //Console.WriteLine($">> producers {JsonConvert.SerializeObject(producers, Formatting.Indented)}");
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var partialObligationsForRun = await GetPartialObligations(runId, materials, producers);

            var result = new CalcResultPartialObligations
            {
                TitleHeader = new CalcResultPartialObligationHeader
                {
                    Name = CalcResultPartialObligationHeaders.PartialObligations,
                    ColumnIndex = 1,
                },
                MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials),
                ColumnHeaders = GetColumnHeaders(materials),
                PartialObligations = partialObligationsForRun
                                        .OrderBy(p => p.ProducerId)
                                        .ThenBy(p => p.Level)
                                        .ThenBy(p => p.SubsidiaryId)
                                        .ToList()
            };
            return (await updateProducerData(resultsRequestDto, producers), result);
        }

        public async Task<List<CalcResultPartialObligation>> GetPartialObligations(int runId, List<MaterialDetail> materials, List<ProducerReportedMaterialsForSubmissionPeriod> producers)
        {
            return await (
                from run in context.CalculatorRuns.AsNoTracking()
                join crodm in context.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in context.CalculatorRunOrganisationDataDetails.AsNoTracking() on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join pd in context.ProducerDetail.Include(x => x.ProducerReportedMaterials) on crodd.OrganisationId equals pd.ProducerId
                where run.Id == runId && crodd.ObligationStatus == ObligationStates.Obligated && crodd.DaysObligated != null && crodd.SubsidiaryId == pd.SubsidiaryId && pd.CalculatorRunId == runId
                let daysInYear = DateTime.IsLeapYear(crodm.RelativeYear.Value) ? 366 : 365
                let partialAmount = crodd.DaysObligated != null ? (decimal)crodd.DaysObligated! / daysInYear : 1
                select new CalcResultPartialObligation
                {
                    ProducerId = pd.ProducerId,
                    SubsidiaryId = pd.SubsidiaryId,
                    ProducerName = pd.ProducerName,
                    TradingName = pd.TradingName,
                    Level = pd.SubsidiaryId != null ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                    SubmissionYear = crodm.RelativeYear.Value.ToString(),
                    DaysInSubmissionYear = daysInYear,
                    JoiningDate = crodd.JoinerDate,
                    DaysObligated = crodd.DaysObligated,
                    ObligatedPercentage = (partialAmount * 100).ToString("F2") + "%",
                    PartialObligationTonnageByMaterial = GetPartialObligationTonnages(pd, materials, partialAmount, producers)
                }
            ).ToListAsync();
        }


        public static Dictionary<string, CalcResultPartialObligationTonnage> GetPartialObligationTonnages(ProducerDetail producer, IEnumerable<MaterialDetail> materials, decimal partialAmount, List<ProducerReportedMaterialsForSubmissionPeriod> producers)
        {
            CalcResultPartialObligationTonnage ToCalcResultPartialObligationTonnage(List<ProducerReportedMaterial> reportedMaterials)
            {
                Console.WriteLine($">> hh.Count {JsonConvert.SerializeObject(reportedMaterials.Where(rm => rm.PackagingType == "HH").Select(rm => (rm.ProducerDetailId, rm.SubmissionPeriod, rm.PackagingType, rm.PackagingTonnage)), Formatting.Indented)}");
                Console.WriteLine($">> hh.Count {JsonConvert.SerializeObject(reportedMaterials.Where(rm => rm.PackagingType == "HH").Count(), Formatting.Indented)}");
                var hh = reportedMaterials.Where(rm => rm.PackagingType == "HH").Select(rm => rm.PackagingTonnage).Sum();
                var pb = reportedMaterials.Where(rm => rm.PackagingType == "PB").Select(rm => rm.PackagingTonnage).Sum();
                var hdc = reportedMaterials.Where(rm => rm.PackagingType == "HDC").Select(rm => rm.PackagingTonnage).Sum();
                var smcw = reportedMaterials.Where(rm => rm.PackagingType == "CW").Select(rm => rm.PackagingTonnage).Sum();
                var scaledHh = Math.Round(partialAmount * hh, 3);
                var scaledPb = Math.Round(partialAmount * pb, 3);
                var scaledHdc = Math.Round(partialAmount * hdc, 3);
                var scaledSmcw = Math.Round(partialAmount * smcw, 3);
                return new CalcResultPartialObligationTonnage
                {
                    ReportedHouseholdPackagingWasteTonnage = hh,
                    ReportedPublicBinTonnage = pb,
                    TotalReportedTonnage = hh + pb + hdc,
                    ReportedSelfManagedConsumerWasteTonnage = smcw,
                    NetReportedTonnage = hh + pb + hdc - smcw, // TODO yikes - rag...
                    PartialReportedHouseholdPackagingWasteTonnage = scaledHh,
                    PartialReportedPublicBinTonnage = scaledPb,
                    PartialTotalReportedTonnage = scaledHh + scaledPb + scaledHdc,
                    PartialReportedSelfManagedConsumerWasteTonnage = scaledSmcw,
                    PartialNetReportedTonnage = scaledHh + scaledPb + scaledHdc - scaledSmcw, // TODO yikes - rag...
                    HouseholdDrinksContainersTonnageGlass = hdc,
                    PartialHouseholdDrinksContainersTonnageGlass = scaledHdc
                };
            }

            return producers
                .Where(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId)
                .GroupBy(p => (p.ProducerId, p.SubsidiaryId))
                .SelectMany(psGroup =>
                    psGroup
                      .SelectMany(p => p.ReportedMaterials)
                      .GroupBy(rm => rm.MaterialId)
                        .Select(mGroup => {
                            Console.WriteLine($">> q.Key: {psGroup.Key.ProducerId}, {psGroup.Key.SubsidiaryId} looking for {JsonConvert.SerializeObject(mGroup.Key, Formatting.Indented)}");
                            return (materials.First(m => m.Id == mGroup.Key).Code, ToCalcResultPartialObligationTonnage(mGroup.ToList()));
                        }
                    )
                )
                .ToDictionary();
        }
/*
        public static CalcResultPartialObligationTonnage GetPartialObligationTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedForMaterial, decimal partialAmount, ProducerDetail producer, List<ProducerReportedMaterialsForSubmissionPeriod> producers)
        {
            decimal GetReportedTonnage(string packagingType) {
                return reportedForMaterial.Where(p => p.PackagingType == packagingType).Sum(t => t.PackagingTonnage);
            }
            var tonnage = new CalcResultPartialObligationTonnage();

            tonnage.ReportedHouseholdPackagingWasteTonnage = GetReportedTonnage(PackagingTypes.Household);
            tonnage.ReportedPublicBinTonnage = GetReportedTonnage(PackagingTypes.PublicBin);
            tonnage.ReportedSelfManagedConsumerWasteTonnage = GetReportedTonnage(PackagingTypes.ConsumerWaste);

            if (material.Code == MaterialCodes.Glass)
            {
                tonnage.HouseholdDrinksContainersTonnageGlass = GetReportedTonnage(PackagingTypes.HouseholdDrinksContainers);
                tonnage.TotalReportedTonnage = tonnage.ReportedHouseholdPackagingWasteTonnage + tonnage.ReportedPublicBinTonnage + tonnage.HouseholdDrinksContainersTonnageGlass;
            }
            else
            {
                tonnage.TotalReportedTonnage = tonnage.ReportedHouseholdPackagingWasteTonnage + tonnage.ReportedPublicBinTonnage;
            }

            tonnage.NetReportedTonnage = tonnage.TotalReportedTonnage - tonnage.ReportedSelfManagedConsumerWasteTonnage;

            var maybeScaledUpReportedHouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetScaledUpTonnage(producer, material, PackagingTypes.Household, scaledupProducers) ?? tonnage.ReportedHouseholdPackagingWasteTonnage;
            var maybeScaledUpReportedPublicBinTonnage = CalcResultSummaryUtil.GetScaledUpTonnage(producer, material, PackagingTypes.PublicBin, scaledupProducers) ?? tonnage.ReportedPublicBinTonnage;
            var maybeScaledUpSelfManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetScaledUpTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledupProducers) ?? tonnage.ReportedSelfManagedConsumerWasteTonnage;

            tonnage.PartialReportedHouseholdPackagingWasteTonnage = Math.Round(maybeScaledUpReportedHouseholdPackagingWasteTonnage * partialAmount, 3);
            tonnage.PartialReportedPublicBinTonnage = Math.Round(maybeScaledUpReportedPublicBinTonnage * partialAmount, 3);
            tonnage.PartialReportedSelfManagedConsumerWasteTonnage = Math.Round(maybeScaledUpSelfManagedConsumerWasteTonnage * partialAmount, 3);

            if (material.Code == MaterialCodes.Glass)
            {
                var maybeScaledUpHouseholdDrinksContainersTonnageGlass = CalcResultSummaryUtil.GetScaledUpTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledupProducers) ?? tonnage.HouseholdDrinksContainersTonnageGlass;
                tonnage.PartialHouseholdDrinksContainersTonnageGlass = Math.Round(maybeScaledUpHouseholdDrinksContainersTonnageGlass * partialAmount, 3);
                tonnage.PartialTotalReportedTonnage = tonnage.PartialReportedHouseholdPackagingWasteTonnage + tonnage.PartialReportedPublicBinTonnage + tonnage.PartialHouseholdDrinksContainersTonnageGlass;
            }
            else
            {
                tonnage.PartialTotalReportedTonnage = tonnage.PartialReportedHouseholdPackagingWasteTonnage + tonnage.PartialReportedPublicBinTonnage;
            }

            tonnage.PartialNetReportedTonnage = tonnage.PartialTotalReportedTonnage - tonnage.PartialReportedSelfManagedConsumerWasteTonnage;

            return tonnage;
        }
        */

        public static List<CalcResultPartialObligationHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<CalcResultPartialObligationHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultPartialObligationHeader
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

        public static List<CalcResultPartialObligationHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = new List<CalcResultPartialObligationHeader>();

            columnHeaders.AddRange([
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ProducerId },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SubsidiaryId },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ProducerOrSubsidiaryName },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.TradingName },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.Level },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SubmissionYear },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.DaysInSubmissionYear },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.JoiningDate },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ObligatedDays },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ObligatedPercentage }
            ]);

            foreach (var material in materials)
            {
                var columnHeadersList = new List<CalcResultPartialObligationHeader>
                {
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.TotalTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.NetTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialTotalTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage },
                    new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialNetTonnage },
                };

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeadersList.Insert(2, new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersTonnageGlass });
                    columnHeadersList.Insert(8, new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersTonnageGlass });
                }

                columnHeaders.AddRange(columnHeadersList);
            }

            return columnHeaders;
        }
    }
}
