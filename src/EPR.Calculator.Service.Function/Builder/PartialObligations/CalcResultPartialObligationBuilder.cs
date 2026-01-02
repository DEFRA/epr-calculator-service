namespace EPR.Calculator.Service.Function.Builder.PartialObligations
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

        public async Task<CalcResultPartialObligations> ConstructAsync(CalcResultsRequestDto resultsRequestDto, IEnumerable<CalcResultScaledupProducer> scaledupProducers)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await this.context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var partialObligationsForRun = await GetPartialObligations(runId, materials, scaledupProducers);

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

        public async Task<List<CalcResultPartialObligation>> GetPartialObligations(int runId, List<MaterialDetail> materials, IEnumerable<CalcResultScaledupProducer> scaledupProducers)
        {
            var result = await (from run in this.context.CalculatorRuns.AsNoTracking()
                                join crodm in this.context.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                                join crodd in this.context.CalculatorRunOrganisationDataDetails.AsNoTracking() on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                                join pd in this.context.ProducerDetail.Include(x => x.ProducerReportedMaterials) on crodd.OrganisationId equals pd.ProducerId
                                where run.Id == runId && crodd.ObligationStatus == ObligationStates.Obligated && crodd.DaysObligated != null && crodd.SubsidiaryId == pd.SubsidiaryId && pd.CalculatorRunId == runId
                                let daysInYear = DateTime.IsLeapYear(int.Parse(crodm.CalendarYear)) ? 366 : 365
                                let partialAmount = crodd.DaysObligated != null ? (decimal)crodd.DaysObligated! / daysInYear : 1
                                select new CalcResultPartialObligation
                                {
                                    ProducerId = pd.ProducerId,
                                    SubsidiaryId = pd.SubsidiaryId,
                                    ProducerName = pd.ProducerName,
                                    TradingName = pd.TradingName,
                                    Level = pd.SubsidiaryId != null ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                                    SubmissionYear = crodm.CalendarYear,
                                    DaysInSubmissionYear = daysInYear,
                                    JoiningDate = crodd.JoinerDate, 
                                    DaysObligated = crodd.DaysObligated,
                                    ObligatedPercentage = (partialAmount * 100).ToString("F2") + "%", 
                                    PartialObligationTonnageByMaterial = GetPartialObligationTonnages(pd.ProducerId, pd.SubsidiaryId, materials, pd.ProducerReportedMaterials.ToList(), partialAmount, scaledupProducers)
                                }).ToListAsync();

            return result ?? new List<CalcResultPartialObligation>();
        }

        public static Dictionary<string, CalcResultPartialObligationTonnage> GetPartialObligationTonnages(int producerId, string? subsidiaryId, IEnumerable<MaterialDetail> materials, List<ProducerReportedMaterial> producerReportedMaterials, decimal partialAmount, IEnumerable<CalcResultScaledupProducer> scaledupProducers)
        {
            var mats = from l in materials
                       join r in producerReportedMaterials on l.Id equals r.MaterialId into mat
                       select new { l.Code, mat };
            
            return mats.ToDictionary(
                m => m.Code, 
                m => {
                    var maybeScaledUpProducerTonnage = scaledupProducers.FirstOrDefault(s => s.ProducerId == producerId && s.SubsidiaryId == subsidiaryId && !s.IsSubtotalRow && !s.IsTotalRow)?.ScaledupProducerTonnageByMaterial;
                    var maybeScaledUpTonnageForMat = maybeScaledUpProducerTonnage != null && maybeScaledUpProducerTonnage!.TryGetValue(m.Code, out var value) ? value : null;
                    return GetPartialObligationTonnage(m.Code, m.mat.ToList(), partialAmount, maybeScaledUpTonnageForMat);
                }
            );
        }

        public static CalcResultPartialObligationTonnage GetPartialObligationTonnage(string material, List<ProducerReportedMaterial> reportedForMaterial, decimal partialAmount, CalcResultScaledupProducerTonnage? maybeScaledUpTonnageForMaterial){
            decimal GetReportedTonnage(string packagingType) {
                return reportedForMaterial.FirstOrDefault(p => p.PackagingType == packagingType)?.PackagingTonnage ?? 0;
            }
            var tonnage = new CalcResultPartialObligationTonnage();

            tonnage.ReportedHouseholdPackagingWasteTonnage = GetReportedTonnage(PackagingTypes.Household);
            tonnage.ReportedPublicBinTonnage = GetReportedTonnage(PackagingTypes.PublicBin);
            tonnage.ReportedSelfManagedConsumerWasteTonnage = GetReportedTonnage(PackagingTypes.ConsumerWaste);

            if (material == MaterialCodes.Glass)
            {
                tonnage.HouseholdDrinksContainersTonnageGlass = GetReportedTonnage(PackagingTypes.HouseholdDrinksContainers);
                tonnage.TotalReportedTonnage = tonnage.ReportedHouseholdPackagingWasteTonnage + tonnage.ReportedPublicBinTonnage + tonnage.HouseholdDrinksContainersTonnageGlass;
            }
            else
            {
                tonnage.TotalReportedTonnage = tonnage.ReportedHouseholdPackagingWasteTonnage + tonnage.ReportedPublicBinTonnage;
            }

            tonnage.NetReportedTonnage = tonnage.TotalReportedTonnage - tonnage.ReportedSelfManagedConsumerWasteTonnage;
            
            var maybeScaledUpReportedHouseholdPackagingWasteTonnage = maybeScaledUpTonnageForMaterial != null ? maybeScaledUpTonnageForMaterial!.ScaledupReportedHouseholdPackagingWasteTonnage : tonnage.ReportedHouseholdPackagingWasteTonnage;
            var maybeScaledUpReportedPublicBinTonnage = maybeScaledUpTonnageForMaterial != null ? maybeScaledUpTonnageForMaterial!.ScaledupReportedPublicBinTonnage : tonnage.ReportedPublicBinTonnage;
            var maybeScaledUpSelfManagedConsumerWasteTonnage = maybeScaledUpTonnageForMaterial != null ? maybeScaledUpTonnageForMaterial!.ScaledupReportedSelfManagedConsumerWasteTonnage : tonnage.ReportedSelfManagedConsumerWasteTonnage;
            var maybeScaledUpNetReportedTonnage = maybeScaledUpTonnageForMaterial != null ? maybeScaledUpTonnageForMaterial!.ScaledupNetReportedTonnage : tonnage.NetReportedTonnage;
            var maybeScaledUpTotalReportedTonnage = maybeScaledUpTonnageForMaterial != null ? maybeScaledUpTonnageForMaterial!.ScaledupTotalReportedTonnage : tonnage.TotalReportedTonnage;

            tonnage.PartialReportedHouseholdPackagingWasteTonnage = Math.Round(maybeScaledUpReportedHouseholdPackagingWasteTonnage * partialAmount, 3);
            tonnage.PartialReportedPublicBinTonnage = Math.Round(maybeScaledUpReportedPublicBinTonnage * partialAmount, 3);
            tonnage.PartialReportedSelfManagedConsumerWasteTonnage = Math.Round(maybeScaledUpSelfManagedConsumerWasteTonnage * partialAmount, 3);
            
            if (material == MaterialCodes.Glass)
            {
                var maybeScaledUpHouseholdDrinksContainersTonnageGlass = maybeScaledUpTonnageForMaterial != null ? maybeScaledUpTonnageForMaterial!.ScaledupHouseholdDrinksContainersTonnageGlass : tonnage.HouseholdDrinksContainersTonnageGlass;
                tonnage.PartialHouseholdDrinksContainersTonnageGlass = maybeScaledUpHouseholdDrinksContainersTonnageGlass * partialAmount;
            }
            
            tonnage.PartialTotalReportedTonnage = Math.Round(maybeScaledUpTotalReportedTonnage * partialAmount, 3);
            tonnage.PartialNetReportedTonnage = Math.Round(maybeScaledUpNetReportedTonnage * partialAmount, 3);

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
