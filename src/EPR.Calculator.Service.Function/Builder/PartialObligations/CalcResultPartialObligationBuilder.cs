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
            CalcResultPartialObligationTonnage ToTonnage(IEnumerable<ProducerReportedMaterial> mats, string materialCode)
             {
                var isGlass = materialCode == MaterialCodes.Glass;
                var matList = mats.ToList();
                decimal hh, pb, scaledHh, scaledPb;
                decimal? hdc = null, scaledHdc = null;
                RAMTonnage? hhRam = null, pbRam = null, hdcRam = null, scaledHhRam = null, scaledPbRam = null, scaledHdcRam = null;

                if(applyModulation)
                {
                    hhRam = RAMTonnage.ToRAMTonnage(PackagingTypes.Household, matList);
                    hh = hhRam.TotalRamTonnage();
                    pbRam = RAMTonnage.ToRAMTonnage(PackagingTypes.PublicBin, matList);
                    pb = pbRam.TotalRamTonnage();

                    if (isGlass)
                    {
                        hdcRam = RAMTonnage.ToRAMTonnage(PackagingTypes.HouseholdDrinksContainers, matList);
                        hdc = hdcRam.TotalRamTonnage();
                        scaledHdcRam = ToPartialRam(hdcRam, partialAmount);
                        scaledHdc = scaledHdcRam.TotalRamTonnage();
                    }

                    scaledHhRam = ToPartialRam(hhRam, partialAmount);
                    scaledHh = scaledHhRam.TotalRamTonnage();
                    scaledPbRam = ToPartialRam(pbRam, partialAmount);
                    scaledPb = scaledPbRam.TotalRamTonnage();
                }
                else
                {
                    hh = RAMTonnage.GetReportedTonnage(matList, PackagingTypes.Household, rm => rm.PackagingTonnage);
                    pb = RAMTonnage.GetReportedTonnage(matList, PackagingTypes.PublicBin, rm => rm.PackagingTonnage);
                    hdc = isGlass ? RAMTonnage.GetReportedTonnage(matList, PackagingTypes.HouseholdDrinksContainers, rm => rm.PackagingTonnage) : null;
                    scaledHh = Math.Round(hh * partialAmount, 3);
                    scaledPb = Math.Round(pb * partialAmount, 3);
                    scaledHdc = isGlass ? Math.Round(hdc!.Value * partialAmount, 3) : null;
                }

                var smcw = RAMTonnage.GetReportedTonnage(matList, PackagingTypes.ConsumerWaste, rm => rm.PackagingTonnage);
                var scaledSmcw = Math.Round(smcw * partialAmount, 3);
                
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
                    g => ToTonnage(g, materials.First(m => m.Id == g.Key).Code)
                );

            return materials.ToDictionary(
                m => m.Code,
                m => byMaterialCode.TryGetValue(m.Code, out var val) ? val : EmptyPartialTonnage(applyModulation, m.Code)
            );
        }

        private static CalcResultPartialObligationTonnage EmptyPartialTonnage(bool applyModulation, string materialCode)
        {
            var isGlass = materialCode == MaterialCodes.Glass;
            if(applyModulation)
            {
                return new CalcResultPartialObligationTonnage()
                {
                    HouseholdRAMTonnage = new RAMTonnage(),
                    PublicBinRAMTonnage = new RAMTonnage(),
                    HouseholdDrinksContainersTonnage = isGlass ? 0m : null,
                    HouseholdDrinksContainersRAMTonnage = isGlass ? new RAMTonnage() : null,
                    PartialHouseholdRAMTonnage =  new RAMTonnage(),
                    PartialPublicBinRAMTonnage = new RAMTonnage(),
                    PartialHouseholdDrinksContainersTonnage = isGlass ? 0m : null,
                    PartialHouseholdDrinksContainersRAMTonnage = isGlass ? new RAMTonnage() : null,
                };
            } else
            {
                return new CalcResultPartialObligationTonnage()
                {
                    HouseholdDrinksContainersTonnage = isGlass ? 0m : null,
                    PartialHouseholdDrinksContainersTonnage = isGlass ? 0m : null
                };
            }
            
        }

        private static RAMTonnage ToPartialRam(RAMTonnage ram, decimal partialAmount) {
            return new RAMTonnage {
                RedTonnage = Math.Round(ram.RedTonnage * partialAmount, 3),
                AmberTonnage = Math.Round(ram.AmberTonnage * partialAmount, 3),
                GreenTonnage = Math.Round(ram.GreenTonnage * partialAmount, 3),
                RedMedicalTonnage = Math.Round(ram.RedMedicalTonnage * partialAmount, 3),
                AmberMedicalTonnage = Math.Round(ram.AmberMedicalTonnage * partialAmount, 3),
                GreenMedicalTonnage = Math.Round(ram.GreenMedicalTonnage * partialAmount, 3),
            };
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
            var columns = new List<CalcResultPartialObligationHeader>();

            void Add(string name) => columns.Add(new CalcResultPartialObligationHeader { Name = name });
            
            void AddRange(params string[] names)
            {
                foreach (var name in names)
                    Add(name);
            }

            void AddRangeIf(bool condition, params string[] names)
            {
                if (!condition) return;
                AddRange(names);
            }

            Add(CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage);
            AddRangeIf(showModulation, 
                CalcResultPartialObligationHeaders.HouseholdRedTonnage, 
                CalcResultPartialObligationHeaders.HouseholdAmberTonnage,
                CalcResultPartialObligationHeaders.HouseholdGreenTonnage,
                CalcResultPartialObligationHeaders.HouseholdRedMedicalTonnage,
                CalcResultPartialObligationHeaders.HouseholdAmberMedicalTonnage,
                CalcResultPartialObligationHeaders.HouseholdGreenMedicalTonnage
            );
            Add(CalcResultPartialObligationHeaders.PublicBinTonnage);
            AddRangeIf(showModulation,
                CalcResultPartialObligationHeaders.PublicBinRedTonnage,
                CalcResultPartialObligationHeaders.PublicBinAmberTonnage,
                CalcResultPartialObligationHeaders.PublicBinGreenTonnage,
                CalcResultPartialObligationHeaders.PublicBinRedMedicalTonnage,
                CalcResultPartialObligationHeaders.PublicBinAmberMedicalTonnage,
                CalcResultPartialObligationHeaders.PublicBinGreenMedicalTonnage
            );
            if(isGlass) {
                Add(CalcResultPartialObligationHeaders.HouseholdDrinksContainersTonnage);
                AddRangeIf(showModulation,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedMedicalTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberMedicalTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenMedicalTonnage
                );
            }
            AddRange(
                CalcResultPartialObligationHeaders.TotalTonnage,
                CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage
            );
            AddRangeIf(showModulation,
                CalcResultPartialObligationHeaders.PartialHouseholdRedTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdAmberTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdGreenTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdRedMedicalTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdAmberMedicalTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdGreenMedicalTonnage
            );
            Add(CalcResultPartialObligationHeaders.PartialPublicBinTonnage);
            AddRangeIf(showModulation,
                CalcResultPartialObligationHeaders.PartialPublicBinRedTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinAmberTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinGreenTonnage,  
                CalcResultPartialObligationHeaders.PartialPublicBinRedMedicalTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinAmberMedicalTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinGreenMedicalTonnage
            );
            if(isGlass) {
                Add(CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersTonnage);
                AddRangeIf(showModulation,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedMedicalTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberMedicalTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenMedicalTonnage
                );
            }
            AddRange(
                CalcResultPartialObligationHeaders.PartialTotalTonnage,
                CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage
            );

            return columns;
        }
    }
}
