using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.PartialObligations
{
    public interface ICalcResultPartialObligationBuilder
    {
        Task<(List<ProducerDetail>, CalcResultPartialObligations)> ConstructAsync(
            List<MaterialDetail> materialDetails,
            List<ProducerDetail> producerDetails,
            CalcResultsRequestDto resultsRequestDto,
            bool applyModulation
        );
    }

    public class CalcResultPartialObligationBuilder : ICalcResultPartialObligationBuilder
    {
        private readonly ApplicationDBContext dbContext;

        public CalcResultPartialObligationBuilder(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private ProducerReportedMaterial scale(bool applyModulation, ProducerReportedMaterial reportedMaterial, CalcResultPartialObligation partialObligation)
        {
            var p = partialObligation.ObligatedFactor;
            if (!applyModulation || reportedMaterial.PackagingType == PackagingTypes.ConsumerWaste)
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

        public async Task<(List<ProducerDetail>, CalcResultPartialObligations)> ConstructAsync(
            List<MaterialDetail> materialDetails,
            List<ProducerDetail> producerDetails,
            CalcResultsRequestDto resultsRequestDto,
            bool applyModulation
        )
        {
            var runId = resultsRequestDto.RunId;

            var partialObligationsForRun = await GetPartialObligations(runId);
            var obligationsLookup = partialObligationsForRun.ToDictionary(p => (p.ProducerId, p.SubsidiaryId));

            var updatedProducers = producerDetails.Select(pd =>
            {
                if (!obligationsLookup.TryGetValue((pd.ProducerId, pd.SubsidiaryId), out var obligation))
                    return pd;

                obligation.PartialObligationTonnageByMaterial =
                    ComputeTonnageByMaterial(pd.ProducerReportedMaterials, materialDetails, obligation.ObligatedFactor ?? 1m, applyModulation);

                return UpdateReportedMaterials(
                    pd,
                    reportedMaterials => reportedMaterials.Select(rm => scale(applyModulation, rm, obligation)).ToList()
                );
            }).ToList();

            var result = new CalcResultPartialObligations
            {
                TitleHeader = new CalcResultPartialObligationHeader
                {
                    Name = CalcResultPartialObligationHeaders.PartialObligations,
                    ColumnIndex = 1,
                },
                MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materialDetails, applyModulation),
                ColumnHeaders = GetColumnHeaders(materialDetails, applyModulation),
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
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join crodm in dbContext.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking() on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join pd in dbContext.ProducerDetail.AsNoTracking() on crodd.OrganisationId equals pd.ProducerId
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
            decimal partialAmount,
            bool applyModulation)
        {
            CalcResultPartialObligationTonnage ToTonnage(IEnumerable<ProducerReportedMaterial> mats, bool isGlass)
            {
                decimal PartialAmountTo3DP(decimal value) {
                    return Math.Round(value * partialAmount, 3);
                }
                RAMTonnage ToPartialRam(RAMTonnage ram) {
                    return new RAMTonnage {
                        RedTonnage = PartialAmountTo3DP(ram.RedTonnage),
                        AmberTonnage = PartialAmountTo3DP(ram.AmberTonnage),
                        GreenTonnage = PartialAmountTo3DP(ram.GreenTonnage),
                        RedMedicalTonnage = PartialAmountTo3DP(ram.RedMedicalTonnage),
                        AmberMedicalTonnage = PartialAmountTo3DP(ram.AmberMedicalTonnage),
                        GreenMedicalTonnage = PartialAmountTo3DP(ram.GreenMedicalTonnage),
                    };
                }

                decimal hh, pb, scaledHh, scaledPb;
                decimal? hdc = null, scaledHdc = null;
                RAMTonnage? hhRam = null, pbRam = null, hdcRam = null, scaledHhRam = null, scaledPbRam = null, scaledHdcRam = null;

                if(applyModulation)
                {
                    hhRam = RAMTonnage.ToRAMTonnage(PackagingTypes.Household, reportedMaterials.ToList());
                    hh = hhRam.TotalRamTonnage();
                    pbRam = RAMTonnage.ToRAMTonnage(PackagingTypes.PublicBin, reportedMaterials.ToList());
                    pb = pbRam.TotalRamTonnage();

                    if (isGlass)
                    {
                        hdcRam = RAMTonnage.ToRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials.ToList());
                        hdc = hdcRam.TotalRamTonnage();
                        scaledHdcRam = ToPartialRam(hdcRam);
                        scaledHdc = scaledHdcRam.TotalRamTonnage();
                    }
                    scaledHhRam = ToPartialRam(hhRam);
                    scaledHh = scaledHhRam.TotalRamTonnage();
                    scaledPbRam = ToPartialRam(pbRam);
                    scaledPb = scaledPbRam.TotalRamTonnage();
                }
                else
                {
                    hh = RAMTonnage.GetReportedTonnage(reportedMaterials.ToList(), PackagingTypes.Household, rm => rm.PackagingTonnage);
                    pb = RAMTonnage.GetReportedTonnage(reportedMaterials.ToList(), PackagingTypes.PublicBin, rm => rm.PackagingTonnage);
                    hdc = isGlass ? RAMTonnage.GetReportedTonnage(reportedMaterials.ToList(), PackagingTypes.HouseholdDrinksContainers, rm => rm.PackagingTonnage) : null;
                    scaledHh = PartialAmountTo3DP(hh);
                    scaledPb = PartialAmountTo3DP(pb);
                    scaledHdc = isGlass ? PartialAmountTo3DP(hdc!.Value) : null;
                }

                var smcw = RAMTonnage.GetReportedTonnage(reportedMaterials.ToList(), PackagingTypes.ConsumerWaste, rm => rm.PackagingTonnage);
                var scaledSmcw = PartialAmountTo3DP(smcw);
                
                return new CalcResultPartialObligationTonnage
                {
                    HouseholdTonnage = hh,
                    HouseholdRAMTonnage = hhRam,
                    PublicBinTonnage = pb,
                    PublicBinRAMTonnage = pbRam,
                    HouseholdDrinksContainersTonnage = hdc,
                    HouseholdDrinksContainersRAMTonnage = hdcRam,
                    TotalTonnage = hh + pb + (hdc ?? 0),
                    SelfManagedConsumerWasteTonnage = smcw,
                    PartialHouseholdTonnage = scaledHh,
                    PartialHouseholdRAMTonnage = scaledHhRam,
                    PartialPublicBinTonnage = scaledPb,
                    PartialPublicBinRAMTonnage = scaledPbRam,
                    PartialHouseholdDrinksContainersTonnage = scaledHdc,
                    PartialHouseholdDrinksContainersRAMTonnage = scaledHdcRam,
                    PartialTotalTonnage = scaledHh + scaledPb + (scaledHdc ?? 0),
                    PartialSelfManagedConsumerWasteTonnage = scaledSmcw
                };
            }

            var byMaterialCode = reportedMaterials
                .GroupBy(rm => rm.MaterialId)
                .ToDictionary(
                    g => materials.First(m => m.Id == g.Key).Code,
                    g => ToTonnage(g, materials.First(m => m.Id == g.Key).Code == MaterialCodes.Glass)
                );

            var empty = (bool isGlass) => new CalcResultPartialObligationTonnage()
            {
                HouseholdRAMTonnage = applyModulation ? new RAMTonnage() : null,
                PublicBinRAMTonnage = applyModulation ? new RAMTonnage() : null,
                HouseholdDrinksContainersTonnage = isGlass ? 0m : null,
                HouseholdDrinksContainersRAMTonnage = (isGlass && applyModulation) ? new RAMTonnage() : null,
                PartialHouseholdRAMTonnage = applyModulation ? new RAMTonnage() : null,
                PartialPublicBinRAMTonnage = applyModulation ? new RAMTonnage(): null,
                PartialHouseholdDrinksContainersTonnage = isGlass ? 0m : null,
                PartialHouseholdDrinksContainersRAMTonnage = (isGlass && applyModulation) ? new RAMTonnage() : null,
            };

            return materials.ToDictionary(
                m => m.Code,
                m => byMaterialCode.TryGetValue(m.Code, out var val) ? val : empty(m.Code == MaterialCodes.Glass)
            );
        }

        public static IEnumerable<CalcResultPartialObligationHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials, bool showModulation)
        {
            var materialsBreakdownHeaders = new List<CalcResultPartialObligationHeader>();
            var columnIndex = GetInitialHeaders().Count + 1;

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultPartialObligationHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex,
                });

                columnIndex = columnIndex + GetMaterialHeaders(isGlass: material.Code == MaterialCodes.Glass, showModulation).Count;
            }

            return materialsBreakdownHeaders;
        }

        public static List<CalcResultPartialObligationHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials, bool showModulation)
        {
            var columnHeaders = new List<CalcResultPartialObligationHeader>();

            columnHeaders.AddRange(GetInitialHeaders());

            foreach (var material in materials.Select(m => m.Code))
            {
                columnHeaders.AddRange(GetMaterialHeaders(isGlass: material == MaterialCodes.Glass, showModulation));
            }

            return columnHeaders;
        }

        private static List<CalcResultPartialObligationHeader> GetInitialHeaders()
        {
            return new List<CalcResultPartialObligationHeader>
            {
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ProducerId },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SubsidiaryId },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ProducerOrSubsidiaryName },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.TradingName },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.Level },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SubmissionYear },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.DaysInSubmissionYear },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.JoiningDate },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ObligatedDays },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ObligatedPercentage },
            };
        }
        private static List<CalcResultPartialObligationHeader> GetMaterialHeaders(bool isGlass, bool showModulation)
        {
            var columns = new List<CalcResultPartialObligationHeader?>
            {
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage },
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdRedTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdAmberTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdGreenTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdRedMedicalTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdAmberMedicalTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdGreenMedicalTonnage } : null,
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinTonnage },
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinRedTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinAmberTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinGreenTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinRedMedicalTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinAmberMedicalTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinGreenMedicalTonnage } : null,
                isGlass ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedMedicalTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberMedicalTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenMedicalTonnage } : null,  
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.TotalTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage },
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdRedTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdAmberTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdGreenTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdRedMedicalTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdAmberMedicalTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdGreenMedicalTonnage } : null,
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinTonnage },
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinRedTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinAmberTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinGreenTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinRedMedicalTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinAmberMedicalTonnage } : null,
                showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinGreenMedicalTonnage } : null,
                isGlass ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedMedicalTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberMedicalTonnage } : null,
                isGlass && showModulation ? new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenMedicalTonnage } : null,
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialTotalTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage },
            };

            return columns.Where(x => x != null).Select(x => x!).ToList();
        }
    }
}
