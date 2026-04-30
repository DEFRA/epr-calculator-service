using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
            bool showModulations
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
            bool showModulations
        )
        {
            var laDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>();
            var orderId = 1;

            producerData = await GetProducerData(resultsRequestDto);

            var lapcapDetails = lapcapData.CalcResultLapcapDataDetails
                .Where(t => t.OrderId != 1 && t.Name != CalcResultLapcapDataBuilder.CountryApportionment).ToList();

            foreach (var detail in lapcapDetails)
            {
                var laDisposalDetail = new CalcResultLaDisposalCostDataDetail
                {
                    Name = detail.Name,
                    England = detail.EnglandDisposalCost,
                    Wales = detail.WalesDisposalCost,
                    Scotland = detail.ScotlandDisposalCost,
                    NorthernIreland = detail.NorthernIrelandDisposalCost,
                    Total = detail.TotalDisposalCost,
                    ProducerReportedHouseholdPackagingWasteTonnage = GetTonnageDataByMaterial(detail.Name),
                    ReportedPublicBinTonnage = GetReportedPublicBinTonnage(detail.Name),
                    HouseholdDrinkContainers = GetReportedHouseholdDrinksContainerTonnage(detail.Name),
                    OrderId = ++orderId,
                };
                laDisposalDetail.LateReportingTonnage = GetLateReportingTonnageDataByMaterial(laDisposalDetail.Name, lateReportingTonnage.CalcResultLateReportingTonnageDetails.ToList());

                var materialCode = materialDetails.FirstOrDefault(x => x.Name == detail.Name)?.Code;

                laDisposalDetail.ActionedSelfManagedConsumerWasteTonnage =
                    showModulations
                        ? GetActionedSelfManagedConsumerWasteTonnageValue(smcw, laDisposalDetail, materialCode).ToString()
                        : String.Empty;

                laDisposalDetail.ProducerReportedTotalTonnage = GetProducerReportedTotalTonnage(laDisposalDetail, showModulations).ToString();

                laDisposalDetail.DisposalCostPricePerTonne = laDisposalDetail.Name == CommonConstants.Total
                    ? string.Empty
                    : CalculateDisposalCostPricePerTonne(laDisposalDetail);
                laDisposalCostDetails.Add(laDisposalDetail);
            }

            var header = GetHeader(showModulations);
            laDisposalCostDetails.Insert(0, header);

            return new CalcResultLaDisposalCostData
            {
                Name = CommonConstants.LADisposalCostData,
                CalcResultLaDisposalCostDetails = laDisposalCostDetails.AsEnumerable()
            };
        }

        private string GetReportedHouseholdDrinksContainerTonnage(string materialName)
        {
            // Return an empty string if the material name is not "Glass" or "Total"
            if (materialName != MaterialNames.Glass && materialName != CommonConstants.Total)
            {
                return string.Empty;
            }

            decimal producerDataTotal = materialName == CommonConstants.Total
                ? producerData.Where(p => p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage)
                : producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage);

            // Return "0" if the material is "Glass" and there's no data, otherwise return the total tonnage as a string
            return (materialName == MaterialNames.Glass && producerDataTotal == 0) ? EmptyString : producerDataTotal.ToString();
        }

        private string GetReportedPublicBinTonnage(string materialName)
        {
            decimal producerDataTotal = materialName == CommonConstants.Total
                ? producerData.Where(p => p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage)
                : producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage);

            return producerDataTotal.ToString();
        }

        private string GetTonnageDataByMaterial(string materialName)
        {
            decimal producerDataTotal = materialName == CommonConstants.Total
                ? producerData.Where(t => t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage)
                : producerData.Where(t => t.MaterialName == materialName && t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage);

            return producerDataTotal.ToString();
        }

        private static string GetLateReportingTonnageDataByMaterial(string materialName, List<CalcResultLateReportingTonnageDetail> details)
        {
            return details
                .Where(t => t.Name == materialName)
                .Sum(t => t.TotalLateReportingTonnage)
                .ToString();
        }

        private static decimal GetProducerReportedTotalTonnage(CalcResultLaDisposalCostDataDetail detail, bool showModulations)
        {
            return GetDecimalValue(detail.LateReportingTonnage)
                + GetDecimalValue(detail.ProducerReportedHouseholdPackagingWasteTonnage)
                + GetDecimalValue(detail.ReportedPublicBinTonnage)
                + GetDecimalValue(detail.HouseholdDrinkContainers)
                - (showModulations ? GetDecimalValue(detail.ActionedSelfManagedConsumerWasteTonnage): 0);
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
                            material.Code == MaterialCodes.Glass))
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
