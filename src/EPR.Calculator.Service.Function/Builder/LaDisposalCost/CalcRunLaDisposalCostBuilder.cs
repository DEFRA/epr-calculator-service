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
        Task<CalcResultLaDisposalCostData> ConstructAsync(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult, SelfManagedConsumerWaste smcw);
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

        public async Task<CalcResultLaDisposalCostData> ConstructAsync(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult, SelfManagedConsumerWaste smcw)
        {
            var laDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>();
            var orderId = 1;

            await SetProducerData(resultsRequestDto);

            var scaledUpProducerReportedOn = calcResult.CalcResultScaledupProducers.ScaledupProducers?.FirstOrDefault(x => x.IsTotalRow);
            producerData = producerData.Where(t => calcResult.CalcResultScaledupProducers.ScaledupProducers != null && !calcResult.CalcResultScaledupProducers.ScaledupProducers.Any(i => i.ProducerId == t.ProducerDetail?.ProducerId)).ToList();

            var lapcapDetails = calcResult.CalcResultLapcapData.CalcResultLapcapDataDetail
                .Where(t => t.OrderId != 1 && t.Name != CalcResultLapcapDataBuilder.CountryApportionment).ToList();

            foreach (var detail in lapcapDetails)
            {
                var laDiposalDetail = new CalcResultLaDisposalCostDataDetail
                {
                    Name = detail.Name,
                    England = detail.EnglandDisposalCost,
                    Wales = detail.WalesDisposalCost,
                    Scotland = detail.ScotlandDisposalCost,
                    NorthernIreland = detail.NorthernIrelandDisposalCost,
                    Total = detail.TotalDisposalCost,
                    ProducerReportedHouseholdPackagingWasteTonnage = GetTonnageDataByMaterial(detail.Name, scaledUpProducerReportedOn!),
                    ReportedPublicBinTonnage = GetReportedPublicBinTonnage(detail.Name, scaledUpProducerReportedOn!),
                    HouseholdDrinkContainers = GetReportedHouseholdDrinksContainerTonnage(detail.Name, scaledUpProducerReportedOn!),
                    OrderId = ++orderId,
                };
                laDiposalDetail.LateReportingTonnage = GetLateReportingTonnageDataByMaterial(laDiposalDetail.Name, calcResult.CalcResultLateReportingTonnageData.CalcResultLateReportingTonnageDetails.ToList());

                var materialCode = producerData.FirstOrDefault(x => x.MaterialName == detail.Name)?.MaterialCode ?? "";

                laDiposalDetail.ActionedSelfManagedConsumerWasteTonnage = calcResult.ShowModulations
                    ? GetActionedSelfManagedConsumerWasteTonnageValue(smcw, laDiposalDetail, materialCode)
                    : string.Empty;

                laDiposalDetail.ProducerReportedTotalTonnage = GetProducerReportedTotalTonnage(laDiposalDetail, calcResult.ShowModulations);

                laDiposalDetail.DisposalCostPricePerTonne = laDiposalDetail.Name == CommonConstants.Total
                    ? string.Empty
                    : CalculateDisposalCostPricePerTonne(laDiposalDetail);
                laDisposalCostDetails.Add(laDiposalDetail);
            }

            var header = GetHeader(calcResult.ShowModulations);
            laDisposalCostDetails.Insert(0, header);

            return new CalcResultLaDisposalCostData
            {
                Name = CommonConstants.LADisposalCostData,
                CalcResultLaDisposalCostDetails = laDisposalCostDetails.AsEnumerable(),
            };
        }

        private string GetReportedHouseholdDrinksContainerTonnage(string materialName, CalcResultScaledupProducer? scaledUpProducerReportedOn)
        {
            // Return an empty string if the material name is not "Glass" or "Total"
            if (materialName != MaterialNames.Glass && materialName != CommonConstants.Total)
            {
                return string.Empty;
            }

            decimal producerDataTotal = materialName == CommonConstants.Total
                ? producerData.Where(p => p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage)
                : producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage);

            decimal scaledDataTotal = 0;
            if (scaledUpProducerReportedOn is not null)
            {
                scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.TryGetValue(materialName, out var scaledProducerTonnages);
                scaledDataTotal = materialName == CommonConstants.Total
                    ? scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.Values.Sum(t => t.ScaledupHouseholdDrinksContainersTonnageGlass)
                    : (scaledProducerTonnages?.ScaledupHouseholdDrinksContainersTonnageGlass ?? 0);
            }

            // Return "0" if the material is "Glass" and there's no data, otherwise return the total tonnage as a string
            return (materialName == MaterialNames.Glass && (producerDataTotal + scaledDataTotal) == 0) ? EmptyString : (producerDataTotal + scaledDataTotal).ToString();
        }

        private string GetReportedPublicBinTonnage(string materialName, CalcResultScaledupProducer? scaledUpProducerReportedOn)
        {
            decimal producerDataTotal = materialName == CommonConstants.Total
                ? producerData.Where(p => p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage)
                : producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage);

            decimal scaledDataTotal = 0;
            if (scaledUpProducerReportedOn is not null)
            {
                scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.TryGetValue(materialName, out var scaledProducerTonnages);
                scaledDataTotal = materialName == CommonConstants.Total
                    ? scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.Values.Sum(t => t.ScaledupReportedPublicBinTonnage)
                    : (scaledProducerTonnages?.ScaledupReportedPublicBinTonnage ?? 0);
            }

            return (producerDataTotal + scaledDataTotal).ToString();
        }

        private string GetTonnageDataByMaterial(string materialName, CalcResultScaledupProducer? scaledUpProducerReportedOn)
        {
            decimal producerDataTotal = materialName == CommonConstants.Total
                ? producerData.Where(t => t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage)
                : producerData.Where(t => t.MaterialName == materialName && t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage);

            decimal scaledDataTotal = 0;
            if (scaledUpProducerReportedOn is not null)
            {
                scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.TryGetValue(materialName, out var scaledProducerTonnages);
                scaledDataTotal = materialName == CommonConstants.Total
                    ? scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.Values.Sum(t => t.ScaledupReportedHouseholdPackagingWasteTonnage)
                    : (scaledProducerTonnages?.ScaledupReportedHouseholdPackagingWasteTonnage ?? 0);
            }

            return (producerDataTotal + scaledDataTotal).ToString();
        }

        private static string GetLateReportingTonnageDataByMaterial(string materialName, List<CalcResultLateReportingTonnageDetail> details)
        {
            return details
                .Where(t => t.Name == materialName)
                .Sum(t => t.TotalLateReportingTonnage)
                .ToString();
        }

        private static string GetProducerReportedTotalTonnage(CalcResultLaDisposalCostDataDetail detail, bool showModulations)
        {
            var value = GetDecimalValue(detail.LateReportingTonnage)
                + GetDecimalValue(detail.ProducerReportedHouseholdPackagingWasteTonnage)
                + GetDecimalValue(detail.ReportedPublicBinTonnage)
                + GetDecimalValue(detail.HouseholdDrinkContainers)
                - (showModulations ? GetDecimalValue(detail.ActionedSelfManagedConsumerWasteTonnage): 0);

            return value.ToString();
        }

        private static string GetActionedSelfManagedConsumerWasteTonnageValue(SelfManagedConsumerWaste smcw, CalcResultLaDisposalCostDataDetail detail, String materialCode)
        {
            return detail.Name == CommonConstants.Total
                ? smcw
                    .OverallTotalPerMaterials
                    .Values
                    .Sum(x => x.ActionedSelfManagedConsumerWasteTonnage ?? 0)
                    .ToString() ?? "0"
                : smcw
                    .OverallTotalPerMaterials[materialCode]
                    .ActionedSelfManagedConsumerWasteTonnage
                    .ToString() ?? "0";
        }

        private static string CalculateDisposalCostPricePerTonne(CalcResultLaDisposalCostDataDetail detail)
        {
            var householdTonnagePlusLateReportingTonnage = GetDecimalValue(detail.ProducerReportedTotalTonnage);
            if (householdTonnagePlusLateReportingTonnage == 0)
            {
                return EmptyString;
            }

            var value = Math.Round(ConvertCurrencyToDecimal(detail.Total) / householdTonnagePlusLateReportingTonnage, 4);
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            return value.ToString("C4", culture);
        }

        private static CalcResultLaDisposalCostDataDetail GetHeader(bool showModulations)
        {
            return new CalcResultLaDisposalCostDataDetail
            {
                Name = CommonConstants.Material,
                England = CommonConstants.England,
                Wales = CommonConstants.Wales,
                Scotland = CommonConstants.Scotland,
                NorthernIreland = CommonConstants.NorthernIreland,
                Total = CommonConstants.Total,
                ProducerReportedHouseholdPackagingWasteTonnage = CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage,
                ReportedPublicBinTonnage = CommonConstants.ReportedPublicBinTonnage,
                HouseholdDrinkContainers = CommonConstants.HouseholdDrinkContainers,
                LateReportingTonnage = CommonConstants.LateReportingTonnage,
                ActionedSelfManagedConsumerWasteTonnage = showModulations ? CommonConstants.ActionedSelfManagedConsumerWasteTonnage : String.Empty,
                ProducerReportedTotalTonnage = showModulations ? CommonConstants.ModulatedProducerReportedTotalTonnage : CommonConstants.ProducerReportedTotalTonnage,
                DisposalCostPricePerTonne = CommonConstants.DisposalCostPricePerTonne,
                OrderId = 1,
            };
        }

        internal static decimal GetDecimalValue(string value)
        {
            var isParseSuccessful = decimal.TryParse(value, CultureInfo.InvariantCulture, out decimal result);
            return isParseSuccessful ? result : 0;
        }

        private static decimal ConvertCurrencyToDecimal(string currency)
        {
            decimal amount;
            decimal.TryParse(currency, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out amount);
            return amount;
        }

        private async Task SetProducerData(CalcResultsRequestDto resultsRequestDto)
        {
            producerData = await (from run in context.CalculatorRuns
                                       join producerDetail in context.ProducerDetail on run.Id equals producerDetail.CalculatorRunId
                                       join producerMaterial in context.ProducerReportedMaterial on producerDetail.Id equals producerMaterial
                                           .ProducerDetailId
                                       join material in context.Material on producerMaterial.MaterialId equals material.Id
                                       where run.Id == resultsRequestDto.RunId &&
                                           producerMaterial.PackagingType != null &&
                                           (
                                               producerMaterial.PackagingType == PackagingTypes.Household ||
                                               producerMaterial.PackagingType == PackagingTypes.PublicBin ||
                                               (
                                                   producerMaterial.PackagingType == PackagingTypes.HouseholdDrinksContainers &&
                                                   material.Code == MaterialCodes.Glass))
                                       select new ProducerData
                                       {
                                           MaterialName = material.Name,
                                           MaterialCode = material.Code,
                                           PackagingType = producerMaterial.PackagingType,
                                           Tonnage = producerMaterial.PackagingTonnage,
                                           ProducerDetail = producerMaterial.ProducerDetail,
                                       }).ToListAsync();
        }
    }
}