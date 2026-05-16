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
            producerData = new List<ProducerData>();
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
            var orderId = 1;

            producerData = await GetProducerData(resultsRequestDto);

            var lapcapDetails = lapcapData.CalcResultLapcapDataDetails.ToList();

            foreach (var detail in lapcapDetails)
            {
                var laDisposalDetail = new CalcResultLaDisposalCostDataDetail
                {
                    Name                     = detail.Name,
                    England                  = detail.EnglandCost,
                    Wales                    = detail.WalesCost,
                    Scotland                 = detail.ScotlandCost,
                    NorthernIreland          = detail.NorthernIrelandCost,
                    Total                    = detail.TotalCost,
                    ProducerReportedHouseholdPackagingWasteTonnage = GetTonnageDataByMaterial(detail.Name),
                    ReportedPublicBinTonnage = GetReportedPublicBinTonnage(detail.Name),
                    HouseholdDrinkContainers = GetReportedHouseholdDrinksContainerTonnage(detail.Name),
                    OrderId                  = ++orderId,
                };
                laDisposalDetail.LateReportingTonnage = GetLateReportingTonnageDataByMaterial(laDisposalDetail.Name, lateReportingTonnage.CalcResultLateReportingTonnageDetails.ToList());

                var materialCode = materialDetails.FirstOrDefault(x => x.Name == detail.Name)?.Code;

                laDisposalDetail.ActionedSelfManagedConsumerWasteTonnage =
                    applyModulation
                        ? GetActionedSelfManagedConsumerWasteTonnageValue(smcw, laDisposalDetail, materialCode)
                        : null;

                laDisposalDetail.ProducerReportedTotalTonnage = GetProducerReportedTotalTonnage(laDisposalDetail, applyModulation);

                laDisposalDetail.DisposalCostPricePerTonne = laDisposalDetail.Name == CommonConstants.Total
                    ? null
                    : CalculateDisposalCostPricePerTonne(laDisposalDetail);
                laDisposalCostDetails.Add(laDisposalDetail);
            }

            return new CalcResultLaDisposalCostData
            {
                CalcResultLaDisposalCostDetails = laDisposalCostDetails
            };
        }

        private decimal? GetReportedHouseholdDrinksContainerTonnage(string materialName)
        {
            // Return an empty string if the material name is not "Glass" or "Total"
            if (materialName != MaterialNames.Glass && materialName != CommonConstants.Total)
            {
                return null;
            }

            decimal producerDataTotal = materialName == CommonConstants.Total
                ? producerData.Where(p => p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage)
                : producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage);

            // Return "0" if the material is "Glass" and there's no data, otherwise return the total tonnage as a string
            return (materialName == MaterialNames.Glass && producerDataTotal == 0) ? null : producerDataTotal;
        }

        private decimal GetReportedPublicBinTonnage(string materialName)
        {
            return materialName == CommonConstants.Total
                ? producerData.Where(p => p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage)
                : producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage);
        }

        private decimal GetTonnageDataByMaterial(string materialName)
        {
            return materialName == CommonConstants.Total
                ? producerData.Where(t => t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage)
                : producerData.Where(t => t.MaterialName == materialName && t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage);
        }

        private static decimal GetLateReportingTonnageDataByMaterial(string materialName, List<CalcResultLateReportingTonnageDetail> details)
        {
            return details
                .Where(t => t.Name == materialName)
                .Sum(t => t.TotalLateReportingTonnage);
        }

        private static decimal GetProducerReportedTotalTonnage(CalcResultLaDisposalCostDataDetail detail, bool applyModulation)
        {
            return (detail.LateReportingTonnage ?? 0m)
                 + detail.ProducerReportedHouseholdPackagingWasteTonnage
                 + detail.ReportedPublicBinTonnage
                 + (detail.HouseholdDrinkContainers ?? 0m)
                 - (applyModulation ? detail.ActionedSelfManagedConsumerWasteTonnage ?? 0m : 0m);
        }

        private static decimal GetActionedSelfManagedConsumerWasteTonnageValue(
            SelfManagedConsumerWaste smcw,
            CalcResultLaDisposalCostDataDetail detail,
            string? materialCode)
        {
            if (detail.Name == CommonConstants.Total)
            {
                return smcw
                    .OverallTotalPerMaterials
                    .Values
                    .Sum(x => x.ActionedSelfManagedConsumerWasteTonnage ?? 0);
            }

            if (materialCode == null)
                return 0;

            return smcw
                .OverallTotalPerMaterials[materialCode]
                .ActionedSelfManagedConsumerWasteTonnage ?? 0;
        }

        private static decimal? CalculateDisposalCostPricePerTonne(CalcResultLaDisposalCostDataDetail detail)
        {
            var householdTonnagePlusLateReportingTonnage = detail.ProducerReportedTotalTonnage ?? 0m;
            if (householdTonnagePlusLateReportingTonnage == 0m)
            {
                return null;
            }

            return Math.Round(detail.Total / householdTonnagePlusLateReportingTonnage, 4);
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
