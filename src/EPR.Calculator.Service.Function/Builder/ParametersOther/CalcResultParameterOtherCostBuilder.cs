namespace EPR.Calculator.Service.Function.Builder.ParametersOther
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultParameterOtherCostBuilder : ICalcResultParameterOtherCostBuilder
    {
        public const string SchemeAdminOperatingCost = "Scheme administrator operating costs";
        public const string LaPrepCharge = "Local authority data preparation costs";
        public const string FourCountryApportionmentPercentage = "4 Country Apportionment %s";
        public const string SchemeSetupCost = "Scheme setup costs";
        public const string BadDebtProvision = "Bad debt provision";
        private const string SchemeSetupYearlyCostHeader = "5 Scheme set up cost Yearly Cost";
        private const string BadDebtProvisionHeader = "6 Bad Debt Provision";
        private const string SaOperatingCostHeader = "3 SA Operating Costs";
        private const string LaDataPrepChargeHeader = "4 LA Data Prep Charge";
        private readonly ApplicationDBContext context;
        private readonly ICalcCountryApportionmentService calcCountryApportionmentService;

        public CalcResultParameterOtherCostBuilder(ApplicationDBContext context,
            ICalcCountryApportionmentService calcCountryApportionmentService) 
        {
            this.context = context;
        }

        public async Task<CalcResultParameterOtherCost> Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            var results = await (from run in this.context.CalculatorRuns
                           join defaultMaster in this.context.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals defaultMaster.Id
                           join defaultDetail in this.context.DefaultParameterSettingDetail on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
                           join defaultTemplate in this.context.DefaultParameterTemplateMasterList on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                           where run.Id == resultsRequestDto.RunId
                           select new DefaultParamResultsClass
                           {
                               ParameterValue = defaultDetail.ParameterValue,
                               ParameterCategory = defaultTemplate.ParameterCategory,
                               ParameterType = defaultTemplate.ParameterType,
                           }).ToListAsync();

            var schemeAdminCosts = results.Where(x => x.ParameterType == SchemeAdminOperatingCost);

            var other = new CalcResultParameterOtherCost
            {
                Name = CommonConstants.ParametersOther,
            };

            var saDetails = new List<CalcResultParameterOtherCostDetail>();
            var saOperatinCostHeader = new CalcResultParameterOtherCostDetail
            {
                England = CommonConstants.England,
                Wales = CommonConstants.Wales,
                Scotland = CommonConstants.Scotland,
                NorthernIreland = CommonConstants.NorthernIreland,
                Total = CommonConstants.Total,
            };
            saDetails.Add(saOperatinCostHeader);

            var saOperatingCost = GetPrepCharge(SaOperatingCostHeader, 2, schemeAdminCosts);
            saDetails.Add(saOperatingCost);
            other.SaOperatingCost = saDetails;

            var lapPrepCharges = results.Where(x => x.ParameterType == LaPrepCharge);
            var laDataPrepCharges = new List<CalcResultParameterOtherCostDetail>();
            var laDataPrep = GetPrepCharge(LaDataPrepChargeHeader, 1, lapPrepCharges);
            laDataPrepCharges.Add(laDataPrep);
            var countryApportionment = GetCountryApportionment(laDataPrep);
            laDataPrepCharges.Add(countryApportionment);
            other.Details = laDataPrepCharges;

            var schemeSetUpCharges = results.Where(x => x.ParameterType == SchemeSetupCost);
            var schemeSetupCharge = GetPrepCharge(SchemeSetupYearlyCostHeader, 1, schemeSetUpCharges);
            other.SchemeSetupCost = schemeSetupCharge;

            var badDebtValue = results.Single(x => x.ParameterType == BadDebtProvision).ParameterValue;
            other.BadDebtValue = badDebtValue;
            other.BadDebtProvision = new KeyValuePair<string, string> (BadDebtProvisionHeader, $"{badDebtValue:0.00}%");

            var materialityHeader = new CalcResultMateriality
            {
                SevenMateriality = "7 Materiality",
                Amount = "Amount £s",
                Percentage = "%",
            };

            var materialities = new List<CalcResultMateriality>();
            materialities.Add(materialityHeader);

            var materialityResults = results.Where(x => x.ParameterType == "Materiality threshold");

            var amountIncrease = materialityResults.Single(x => x.ParameterCategory == "Amount Increase");
            var amountDecrease = materialityResults.Single(x => x.ParameterCategory == "Amount Decrease");
            var percentageDecrease = materialityResults.Single(x => x.ParameterCategory == "Percent Decrease");
            var precentageIncrease = materialityResults.Single(x => x.ParameterCategory == "Percent Increase");

            var materialityIncrease = new CalcResultMateriality
            {
                SevenMateriality = "Increase",
                AmountValue = amountIncrease.ParameterValue,
                PercentageValue = precentageIncrease.ParameterValue,
            };
            materialityIncrease.Amount = materialityIncrease.AmountValue.ToString("C", culture);
            materialityIncrease.Percentage = $"{materialityIncrease.PercentageValue:0.00}%";
            materialities.Add (materialityIncrease);

            var materialityDecrease = new CalcResultMateriality
            {
                SevenMateriality = "Decrease",
                AmountValue = amountDecrease.ParameterValue,
                PercentageValue = percentageDecrease.ParameterValue,
            };
            materialityDecrease.Amount = materialityDecrease.AmountValue.ToString("C", culture);
            materialityDecrease.Percentage = $"{materialityDecrease.PercentageValue:0.00}%";
            materialities.Add(materialityDecrease);
            other.Materiality = materialities;

            materialities.Add(new CalcResultMateriality
            {
                SevenMateriality = "8 Tonnage Change",
                Percentage = "%",
                Amount = "Amount £s",
            });

            var tonnageResults = results.Where(x => x.ParameterType == "Tonnage change threshold");
            var tonIncrease = tonnageResults.Single(x => x.ParameterCategory == "Amount Increase");
            var tonDecrease = tonnageResults.Single(x => x.ParameterCategory == "Amount Decrease");
            var tonPercentageDecrease = tonnageResults.Single(x => x.ParameterCategory == "Percent Decrease");
            var tonPrecentageIncrease = tonnageResults.Single(x => x.ParameterCategory == "Percent Increase");

            var tonnageIncrease = new CalcResultMateriality
            {
                SevenMateriality = "Increase",
                AmountValue = tonIncrease.ParameterValue,
                PercentageValue = tonPrecentageIncrease.ParameterValue,
                Amount = $"{tonIncrease.ParameterValue.ToString("C", culture)}",
                Percentage = $"{tonPrecentageIncrease.ParameterValue:0.00}%",
            };
            materialities.Add(tonnageIncrease);

            var tonnageDecrease = new CalcResultMateriality
            {
                SevenMateriality = "Decrease",
                AmountValue = tonDecrease.ParameterValue,
                PercentageValue = tonPercentageDecrease.ParameterValue,
                Amount = $"{tonDecrease.ParameterValue.ToString("C", culture)}",
                Percentage = $"{tonPercentageDecrease.ParameterValue:0.00}%",
            };
            materialities.Add(tonnageDecrease);
            other.Materiality = materialities;

            var countries = await this.context.Country.ToListAsync();

            var costType = await this.context.CostType.SingleAsync(x => x.Name == "LA Data Prep Charge");
            var costTypeId = costType.Id;

            if (!resultsRequestDto.IsBillingFile)
            {
                await this.calcCountryApportionmentService.SaveChangesAsync(new CalcCountryApportionmentServiceDto
                {
                    RunId = resultsRequestDto.RunId,
                    Countries = countries,
                    CostTypeId = costTypeId,
                    EnglandCost = laDataPrep.EnglandValue,
                    NorthernIrelandCost = laDataPrep.NorthernIrelandValue,
                    ScotlandCost = laDataPrep.ScotlandValue,
                    WalesCost = laDataPrep.WalesValue,
                });
            }

            return other;
        }

        private static CalcResultParameterOtherCostDetail GetCountryApportionment(CalcResultParameterOtherCostDetail laDataPrep)
        {
            var total = laDataPrep.EnglandValue + laDataPrep.NorthernIrelandValue + laDataPrep.WalesValue +
                        laDataPrep.ScotlandValue;
            var otherCostDetail = new CalcResultParameterOtherCostDetail
            {
                Name = FourCountryApportionmentPercentage,
                EnglandValue = total != 0 ? ((laDataPrep.EnglandValue / total) * 100) : 0M,
                NorthernIrelandValue = total != 0 ? (laDataPrep.NorthernIrelandValue / total) * 100 : 0M,
                ScotlandValue = total != 0 ? (laDataPrep.ScotlandValue / total) * 100 : 0M,
                WalesValue = total != 0 ? (laDataPrep.WalesValue / total) * 100 : 0M,
                OrderId = 2,
                TotalValue = 100M,
            };
            otherCostDetail.England = $"{otherCostDetail.EnglandValue:0.00000000}%";
            otherCostDetail.NorthernIreland = $"{otherCostDetail.NorthernIrelandValue:0.00000000}%";
            otherCostDetail.Scotland = $"{otherCostDetail.ScotlandValue:0.00000000}%";
            otherCostDetail.Wales = $"{otherCostDetail.WalesValue:0.00000000}%";
            otherCostDetail.Total = $"{otherCostDetail.TotalValue:0.00000000}%";

            return otherCostDetail;
        }

        private static CalcResultParameterOtherCostDetail GetPrepCharge(string name, int orderId, IEnumerable<DefaultParamResultsClass> lapPrepCharges)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            var otherCostDetail = new CalcResultParameterOtherCostDetail
            {
                Name = name,
                EnglandValue = lapPrepCharges.Single(cost => cost.ParameterCategory == "England").ParameterValue,
                NorthernIrelandValue = lapPrepCharges.Single(cost => cost.ParameterCategory == "Northern Ireland").ParameterValue,
                ScotlandValue = lapPrepCharges.Single(cost => cost.ParameterCategory == "Scotland").ParameterValue,
                WalesValue = lapPrepCharges.Single(cost => cost.ParameterCategory == "Wales").ParameterValue,
                OrderId = orderId,
            };
            otherCostDetail.TotalValue = otherCostDetail.EnglandValue + otherCostDetail.ScotlandValue + otherCostDetail.WalesValue + otherCostDetail.NorthernIrelandValue;
            otherCostDetail.England = otherCostDetail.EnglandValue.ToString("C", culture);
            otherCostDetail.Wales = otherCostDetail.WalesValue.ToString("C", culture);
            otherCostDetail.NorthernIreland = otherCostDetail.NorthernIrelandValue.ToString("C", culture);
            otherCostDetail.Scotland = otherCostDetail.ScotlandValue.ToString("C", culture);
            otherCostDetail.Total = otherCostDetail.TotalValue.ToString("C", culture);

            return otherCostDetail;
        }
    }
}
