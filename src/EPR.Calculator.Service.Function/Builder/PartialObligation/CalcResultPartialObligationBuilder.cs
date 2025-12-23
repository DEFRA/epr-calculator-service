namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;


    public class CalcResultPartialObligationBuilder : ICalcResultPartialObligationBuilder
    {

        private readonly ApplicationDBContext context;
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 11;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;

        public CalcResultPartialObligationBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultPartialObligations> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await this.context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var partialObligationsForRun = await GetPartialObligations(runId, materials);

            return new CalcResultPartialObligations
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
        }

        public async Task<List<CalcResultPartialObligation>> GetPartialObligations(int runId, List<MaterialDetail> materials)
        {
            //Need to consider indexes vs post filter
            //Improve null scenarios
            //Leaver date?
            //Get year from org data instead (add column)?
            //Date format different to file? DateOnly?
            var result = await (from run in this.context.CalculatorRuns.AsNoTracking()
                                join crodm in this.context.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                                join crodd in this.context.CalculatorRunOrganisationDataDetails.AsNoTracking() on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                                join pd in this.context.ProducerDetail.Include(x => x.ProducerReportedMaterials) on crodd.OrganisationId equals pd.ProducerId
                                where run.Id == runId && crodd.ObligationStatus == ObligationStates.Obligated && crodd.DaysObligated != null && crodd.SubsidiaryId == pd.SubsidiaryId
                                let daysInYear = DateTime.IsLeapYear(int.Parse(crodm.CalendarYear)) ? 366 : 365
                                let partialAmount = crodd.DaysObligated != null ? (decimal)crodd.DaysObligated! / daysInYear : 1
                                select new CalcResultPartialObligation
                                {
                                    ProducerId = pd.ProducerId,
                                    SubsidiaryId = pd.SubsidiaryId!,
                                    ProducerName = pd.ProducerName!,
                                    TradingName = pd.TradingName!,
                                    Level = pd.SubsidiaryId != null ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                                    SubmissionYear = crodm.CalendarYear,
                                    DaysInSubmissionYear = daysInYear,
                                    JoiningDate = crodd.JoinerDate, 
                                    DaysObligated = crodd.DaysObligated,
                                    ObligatedPercentage = (partialAmount * 100).ToString("F2") + "%", 
                                    PartialObligationTonnageByMaterial = GetPartialObligationTonnages(materials, pd.ProducerReportedMaterials.ToList(), partialAmount)
                                }).ToListAsync();

            return result ?? new List<CalcResultPartialObligation>();
        }

        public Dictionary<string, CalcResultPartialObligationTonnage> GetPartialObligationTonnages(IEnumerable<MaterialDetail> materials, List<ProducerReportedMaterial> producerReportedMaterials, decimal partialAmount)
        {
            var mats = from l in materials
                       join r in producerReportedMaterials on l.Id equals r.MaterialId into mat
                       select new { l.Code, mat };

            return mats.ToDictionary(m => m.Code, m => GetPartialObligationTonnage(m.Code, m.mat.ToList(), partialAmount));
        }

        public CalcResultPartialObligationTonnage GetPartialObligationTonnage(string material, List<ProducerReportedMaterial> reportedForMaterial, decimal partialAmount){
            // Are these all converted from kg to tonne already?
            // Do we need the sum? or reportedMaterial for type is already summed? i.e. 1 HH AL
            var tonnage = new CalcResultPartialObligationTonnage();

            tonnage.ReportedHouseholdPackagingWasteTonnage = reportedForMaterial.FirstOrDefault(p => p.PackagingType == PackagingTypes.Household)?.PackagingTonnage ?? 0;
            tonnage.ReportedPublicBinTonnage = reportedForMaterial.FirstOrDefault(p => p.PackagingType == PackagingTypes.PublicBin)?.PackagingTonnage ?? 0;
            tonnage.ReportedSelfManagedConsumerWasteTonnage = reportedForMaterial.FirstOrDefault(p => p.PackagingType == PackagingTypes.ConsumerWaste)?.PackagingTonnage ?? 0;

            if (material == MaterialCodes.Glass)
            {
                tonnage.HouseholdDrinksContainersTonnageGlass = reportedForMaterial.FirstOrDefault(p => p.PackagingType == PackagingTypes.HouseholdDrinksContainers)?.PackagingTonnage ?? 0;
                tonnage.TotalReportedTonnage = tonnage.ReportedHouseholdPackagingWasteTonnage + tonnage.ReportedPublicBinTonnage + tonnage.HouseholdDrinksContainersTonnageGlass;
            }
            else
            {
                tonnage.TotalReportedTonnage = tonnage.ReportedHouseholdPackagingWasteTonnage + tonnage.ReportedPublicBinTonnage;
            }

            tonnage.NetReportedTonnage = tonnage.TotalReportedTonnage - tonnage.ReportedSelfManagedConsumerWasteTonnage;
            
            tonnage.PartialReportedHouseholdPackagingWasteTonnage = Math.Round(tonnage.ReportedHouseholdPackagingWasteTonnage * partialAmount, 3);
            tonnage.PartialReportedPublicBinTonnage = Math.Round(tonnage.ReportedPublicBinTonnage * partialAmount, 3);
            if (material == MaterialCodes.Glass)
            {
                tonnage.PartialHouseholdDrinksContainersTonnageGlass = tonnage.HouseholdDrinksContainersTonnageGlass * partialAmount;
            }
            tonnage.PartialTotalReportedTonnage = Math.Round(tonnage.TotalReportedTonnage * partialAmount, 3);
            tonnage.PartialReportedSelfManagedConsumerWasteTonnage = Math.Round(tonnage.ReportedSelfManagedConsumerWasteTonnage * partialAmount, 3);
            tonnage.PartialNetReportedTonnage = Math.Round(tonnage.NetReportedTonnage * partialAmount, 3);

            return tonnage;
        }

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
