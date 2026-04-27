using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.PartialObligations
{
    public interface ICalcResultPartialObligationBuilder
    {
        Task<(List<ProducerDetail>, CalcResultPartialObligations)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerDetail> producerDetails);
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

        private ProducerReportedMaterial scale(ProducerReportedMaterial reportedMaterial, CalcResultPartialObligation partialObligation)
        {
            var p = partialObligation.ObligatedFactor;
            if (reportedMaterial.SubmissionPeriod.StartsWith("2024")) // TODO or use ShowModulation = false
            {
                return new ProducerReportedMaterial
                {
                    Id = reportedMaterial.Id,
                    MaterialId = reportedMaterial.MaterialId,
                    ProducerDetailId = reportedMaterial.ProducerDetailId,
                    PackagingType = reportedMaterial.PackagingType,
                    PackagingTonnage = Math.Round(p * reportedMaterial.PackagingTonnage ?? 0m, 3),
                    SubmissionPeriod = reportedMaterial.SubmissionPeriod,
                    ProducerDetail = reportedMaterial.ProducerDetail,
                    Material = reportedMaterial.Material
                };
            }
            else
            {
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
                    PackagingTonnageRedMedical = rm,
                    PackagingTonnageAmberMedical = am,
                    PackagingTonnageGreenMedical = gm,
                    SubmissionPeriod = reportedMaterial.SubmissionPeriod,
                    ProducerDetail = reportedMaterial.ProducerDetail,
                    Material = reportedMaterial.Material
                };
            }
        }

        // TODO move this to a utility
        public static ProducerDetail UpdateReportedMaterials(
            ProducerDetail pd,
            Func<ICollection<ProducerReportedMaterial>, ICollection<ProducerReportedMaterial>> f
        )
        {
            var updatedReportedMaterials = f(pd.ProducerReportedMaterials);
            pd.ProducerReportedMaterials.Clear();
            foreach (var item in updatedReportedMaterials) pd.ProducerReportedMaterials.Add(item);
            return pd;
        }

        public async Task<(List<ProducerDetail>, CalcResultPartialObligations)> ConstructAsync(CalcResultsRequestDto resultsRequestDto, List<ProducerDetail> producerDetails)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var partialObligationsForRun = await GetPartialObligations(runId);
            var obligationsLookup = partialObligationsForRun.ToDictionary(p => (p.ProducerId, p.SubsidiaryId));

            var updatedProducers = producerDetails.Select(pd =>
            {
                if (!obligationsLookup.TryGetValue((pd.ProducerId, pd.SubsidiaryId), out var obligation))
                    return pd;

                obligation.PartialObligationTonnageByMaterial =
                    ComputeTonnageByMaterial(pd.ProducerReportedMaterials, materials, obligation.ObligatedFactor ?? 1m);

                return UpdateReportedMaterials(
                    pd,
                    reportedMaterials => reportedMaterials.Select(rm => scale(rm, obligation)).ToList()
                );
            }).ToList();

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

            return (updatedProducers, result);
        }

        private async Task<List<CalcResultPartialObligation>> GetPartialObligations(int runId)
        {
            return await (
                from run in context.CalculatorRuns.AsNoTracking()
                join crodm in context.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in context.CalculatorRunOrganisationDataDetails.AsNoTracking() on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join pd in context.ProducerDetail.AsNoTracking() on crodd.OrganisationId equals pd.ProducerId
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
                    ObligatedFactor = partialAmount,
                    ObligatedPercentage = (partialAmount * 100).ToString("F2") + "%",
                }
            ).ToListAsync();
        }

        private static Dictionary<string, CalcResultPartialObligationTonnage> ComputeTonnageByMaterial(
            ICollection<ProducerReportedMaterial> reportedMaterials,
            IEnumerable<MaterialDetail> materials,
            decimal partialAmount)
        {
            CalcResultPartialObligationTonnage ToTonnage(IEnumerable<ProducerReportedMaterial> mats)
            {
                var hh   = mats.Where(rm => rm.PackagingType == "HH").Sum(rm => rm.PackagingTonnage);
                var pb   = mats.Where(rm => rm.PackagingType == "PB").Sum(rm => rm.PackagingTonnage);
                var hdc  = mats.Where(rm => rm.PackagingType == "HDC").Sum(rm => rm.PackagingTonnage);
                var smcw = mats.Where(rm => rm.PackagingType == "CW").Sum(rm => rm.PackagingTonnage);
                var scaledHh   = Math.Round(partialAmount * hh, 3);
                var scaledPb   = Math.Round(partialAmount * pb, 3);
                var scaledHdc  = Math.Round(partialAmount * hdc, 3);
                var scaledSmcw = Math.Round(partialAmount * smcw, 3);
                return new CalcResultPartialObligationTonnage
                {
                    ReportedHouseholdPackagingWasteTonnage = hh,
                    ReportedPublicBinTonnage = pb,
                    TotalReportedTonnage = hh + pb + hdc,
                    ReportedSelfManagedConsumerWasteTonnage = smcw,
                    NetReportedTonnage = hh + pb + hdc - smcw, // TODO remove this field - will not be shown
                    PartialReportedHouseholdPackagingWasteTonnage = scaledHh,
                    PartialReportedPublicBinTonnage = scaledPb,
                    PartialTotalReportedTonnage = scaledHh + scaledPb + scaledHdc,
                    PartialReportedSelfManagedConsumerWasteTonnage = scaledSmcw,
                    PartialNetReportedTonnage = scaledHh + scaledPb + scaledHdc - scaledSmcw, // TODO remove this field - will not be shown
                    HouseholdDrinksContainersTonnageGlass = hdc,
                    PartialHouseholdDrinksContainersTonnageGlass = scaledHdc
                };
            }

            var byMaterialCode = reportedMaterials
                .GroupBy(rm => rm.MaterialId)
                .ToDictionary(
                    g => materials.First(m => m.Id == g.Key).Code,
                    g => ToTonnage(g)
                );

            var empty = new CalcResultPartialObligationTonnage();
            return materials.ToDictionary(
                m => m.Code,
                m => byMaterialCode.TryGetValue(m.Code, out var val) ? val : empty
            );
        }

        private static List<CalcResultPartialObligationHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
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

        private static List<CalcResultPartialObligationHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
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
