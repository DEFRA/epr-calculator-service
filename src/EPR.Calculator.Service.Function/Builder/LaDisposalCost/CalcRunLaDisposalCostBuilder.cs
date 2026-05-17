using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.LaDisposalCost
{
    public interface ICalcRunLaDisposalCostBuilder
    {
        Task<CalcResultLaDisposalCostData> ConstructAsync(
            CalcResultsRequestDto resultsRequestDto,
            IEnumerable<MaterialDetail> materialDetails,
            CalcResultLapcapData lapcapData,
            CalcResultLateReportingTonnage lateReportingTonnage,
            SelfManagedConsumerWaste smcw,
            bool applyModulation
        );
    }

    public class CalcRunLaDisposalCostBuilder : ICalcRunLaDisposalCostBuilder
    {
        private const string EmptyString = "0";
        private readonly ApplicationDBContext context;
        private List<ProducerData> producerData;

        internal class ProducerData
        {
            public required string MaterialName { get; set; }

            public required string MaterialCode { get; set; }

            public required string PackagingType { get; set; }

            public decimal Tonnage { get; set; }

            public required ProducerDetail? ProducerDetail { get; set; }
        }

        public CalcRunLaDisposalCostBuilder(ApplicationDBContext context)
        {
            this.context = context;
            this.producerData = new List<ProducerData>();
        }

        public async Task<CalcResultLaDisposalCostData> ConstructAsync(
            CalcResultsRequestDto resultsRequestDto,
            IEnumerable<MaterialDetail> materialDetails,
            CalcResultLapcapData lapcapData,
            CalcResultLateReportingTonnage lateReportingTonnage,
            SelfManagedConsumerWaste smcw,
            bool applyModulation
        )
        {
            var laDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>();

            producerData = await GetProducerData(resultsRequestDto);

            var lapcapDetailsByMaterial =
                lapcapData.ByMaterial.Select(detail =>
                {
                    var materialName = detail.Key.Name;
                    var lateReportingTonnageValue = lateReportingTonnage.CalcResultLateReportingTonnageDetails.Where(t => t.Name == materialName).Sum(t => t.TotalLateReportingTonnage);
                    var producerReportedHouseholdPackagingWasteTonnage = producerData.Where(t => t.MaterialName == materialName && t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage);
                    var reportedPublicBinTonnage                       = producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage);
                    decimal? householdDrinkContainers = materialName == "Glass"
                        ? producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage)
                        : null;
                    decimal? actionedSelfManagedConsumerWasteTonnage = applyModulation ? smcw.OverallTotalPerMaterials[detail.Key.Code].ActionedSelfManagedConsumerWasteTonnage.total ?? 0 : null;
                    var producerReportedTotalTonnage =
                        lateReportingTonnageValue
                            + producerReportedHouseholdPackagingWasteTonnage
                            + reportedPublicBinTonnage
                            + householdDrinkContainers ?? 0m
                            - actionedSelfManagedConsumerWasteTonnage ?? 0m;
                    var laDisposalDetail = new CalcResultLaDisposalCostDataDetail
                    {
                        England                  = detail.Value.England,
                        Wales                    = detail.Value.Wales,
                        Scotland                 = detail.Value.Scotland,
                        NorthernIreland          = detail.Value.NorthernIreland,
                        Total                    = detail.Value.Total,
                        ProducerReportedHouseholdPackagingWasteTonnage = producerReportedHouseholdPackagingWasteTonnage,
                        ReportedPublicBinTonnage = reportedPublicBinTonnage,
                        HouseholdDrinkContainers = householdDrinkContainers,
                        LateReportingTonnage     = lateReportingTonnageValue,
                        ActionedSelfManagedConsumerWasteTonnage = actionedSelfManagedConsumerWasteTonnage,
                        ProducerReportedTotalTonnage = lateReportingTonnageValue
                                                    + producerReportedHouseholdPackagingWasteTonnage
                                                    + reportedPublicBinTonnage
                                                    + (householdDrinkContainers ?? 0m)
                                                    - (actionedSelfManagedConsumerWasteTonnage ?? 0m),
                        DisposalCostPricePerTonne = CalculateDisposalCostPricePerTonne(producerReportedTotalTonnage, detail.Value.Total)
                    };
                    return (detail.Key, laDisposalDetail);
                }).ToDictionary();

            var totalLateReportingTonnageValue = lateReportingTonnage.CalcResultLateReportingTonnageDetails.Where(t => t.Name == "Total").Sum(t => t.TotalLateReportingTonnage);
            var totalProducerReportedHouseholdPackagingWasteTonnage = producerData.Where(t => t.PackagingType == PackagingTypes.Household                ).Sum(t => t.Tonnage);
            var totalReportedPublicBinTonnage                       = producerData.Where(p => p.PackagingType == PackagingTypes.PublicBin                ).Sum(p => p.Tonnage);
            var totalHouseholdDrinkContainers                       = producerData.Where(p => p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage);
            decimal? totalActionedSelfManagedConsumerWasteTonnage = applyModulation ? smcw.OverallTotalPerMaterials.Values.Sum(x => x.ActionedSelfManagedConsumerWasteTonnage.total ?? 0) : null;
            var total = new CalcResultLaDisposalCostDataDetail
                {
                    England                  = lapcapData.Total.England,
                    Wales                    = lapcapData.Total.Wales,
                    Scotland                 = lapcapData.Total.Scotland,
                    NorthernIreland          = lapcapData.Total.NorthernIreland,
                    Total                    = lapcapData.Total.Total,
                    ProducerReportedHouseholdPackagingWasteTonnage = totalProducerReportedHouseholdPackagingWasteTonnage,
                    ReportedPublicBinTonnage  = totalReportedPublicBinTonnage,
                    HouseholdDrinkContainers  = totalHouseholdDrinkContainers,
                    LateReportingTonnage      = totalLateReportingTonnageValue,
                    ActionedSelfManagedConsumerWasteTonnage = applyModulation ? smcw.OverallTotalPerMaterials.Values.Sum(x => x.ActionedSelfManagedConsumerWasteTonnage.total ?? 0) : null,
                    ProducerReportedTotalTonnage = totalLateReportingTonnageValue
                                                + totalProducerReportedHouseholdPackagingWasteTonnage
                                                + totalReportedPublicBinTonnage
                                                + totalHouseholdDrinkContainers
                                                - totalActionedSelfManagedConsumerWasteTonnage ?? 0m
                };

            return new CalcResultLaDisposalCostData
            {
                ByMaterial = lapcapDetailsByMaterial,
                Total      = total
            };
        }

        private static decimal? CalculateDisposalCostPricePerTonne(decimal? producerReportedTotalTonnage, decimal total)
        {
            var householdTonnagePlusLateReportingTonnage = producerReportedTotalTonnage ?? 0m;

            if (householdTonnagePlusLateReportingTonnage == 0m)
            {
                return null;
            }
            return Math.Round(total / householdTonnagePlusLateReportingTonnage, 4);
        }

        private async Task<List<ProducerData>> GetProducerData(CalcResultsRequestDto resultsRequestDto)
        {
            // TODO note returns duplicates for SubmissionPeriod - should remove it from ProducerReportedMaterialProjected - it's not needed
            // TODO why filter PackagingType/Material? should already be done
            return await (
                from run in context.CalculatorRuns
                join producerDetail in context.ProducerDetail on run.Id equals producerDetail.CalculatorRunId
                join producerMaterial in context.ProducerReportedMaterialProjected on producerDetail.Id equals producerMaterial.ProducerDetailId
                join material in context.Material on producerMaterial.MaterialId equals material.Id
                where run.Id == resultsRequestDto.RunId &&
                    producerMaterial.PackagingType != null &&
                    (
                        producerMaterial.PackagingType == PackagingTypes.Household ||
                        producerMaterial.PackagingType == PackagingTypes.PublicBin ||
                        (
                            producerMaterial.PackagingType == PackagingTypes.HouseholdDrinksContainers &&
                            material.Code == MaterialCodes.Glass
                        )
                    )
                select new ProducerData
                {
                    MaterialName = material.Name,
                    MaterialCode = material.Code,
                    PackagingType = producerMaterial.PackagingType,
                    Tonnage = producerMaterial.PackagingTonnage,
                    ProducerDetail = producerMaterial.ProducerDetail,
                }
            ).ToListAsync();
        }
    }
}
