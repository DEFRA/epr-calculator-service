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
                    var materialCode = detail.Key;
                    var materialName = materialDetails.First(m => m.Code == materialCode).Name;
                    var lateReportingTonnageValue = lateReportingTonnage.ByMaterial[materialCode].Total;
                    var producerReportedHouseholdPackagingWasteTonnage = producerData.Where(t => t.MaterialName == materialName && t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage);
                    var reportedPublicBinTonnage                       = producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage);
                    decimal? householdDrinkContainers = materialName == "Glass"
                        ? producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage)
                        : null;
                    decimal? actionedSelfManagedConsumerWasteTonnage = applyModulation ? smcw.OverallTotalPerMaterials[materialCode].ActionedSelfManagedConsumerWasteTonnage.total ?? 0 : null;
                    var totalTonnage =
                        lateReportingTonnageValue
                            + producerReportedHouseholdPackagingWasteTonnage
                            + reportedPublicBinTonnage
                            + (householdDrinkContainers ?? 0)
                            - (actionedSelfManagedConsumerWasteTonnage ?? 0);
                    var disposalCostPricePerTonne =
                       totalTonnage == 0 ? (decimal?)null : Math.Round(detail.Value.Total / totalTonnage, 4);
                    var laDisposalDetail = new CalcResultLaDisposalCostDataDetail
                    {
                        EnglandCost                  = detail.Value.England,
                        WalesCost                    = detail.Value.Wales,
                        ScotlandCost                 = detail.Value.Scotland,
                        NorthernIrelandCost          = detail.Value.NorthernIreland,
                        HouseholdPackagingWasteTonnage          = producerReportedHouseholdPackagingWasteTonnage,
                        PublicBinTonnage                        = reportedPublicBinTonnage,
                        HouseholdDrinkContainersTonnage         = householdDrinkContainers,
                        LateReportingTonnage                    = lateReportingTonnageValue,
                        ActionedSelfManagedConsumerWasteTonnage = actionedSelfManagedConsumerWasteTonnage,
                        TotalTonnage                            = totalTonnage,
                        DisposalCostPricePerTonne               = disposalCostPricePerTonne
                    };
                    return (detail.Key, laDisposalDetail);
                }).ToDictionary();

            return new CalcResultLaDisposalCostData
            {
                ByMaterial = lapcapDetailsByMaterial
            };
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
