using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultParametersOtherJson
    {
        [JsonPropertyName("threeSAOperatingCost")]
        public CountryAmountJson ThreeSAOperatingCost { get; set; } = new CountryAmountJson();

        [JsonPropertyName("fourDataPreparationCharge")]
        public CountryAmountJson FourDataPreparationCharge { get; set; } = new CountryAmountJson();

        [JsonPropertyName("fourCountryApportionmentPercentages")]
        public CountryAmountJson FourCountryApportionmentPercentages { get; set; } = new CountryAmountJson();

        [JsonPropertyName("fiveSchemeSetupCost")]
        public CountryAmountJson FiveSchemeSetupCost { get; set; } = new CountryAmountJson();

        [JsonPropertyName("sixBadDebtProvision")]
        public PercentageJson SixBadDebtProvision { get; set; } = new PercentageJson();

        [JsonPropertyName("sevenMateriality")]
        public ChangeJson SevenMateriality { get; set; } = new ChangeJson();

        [JsonPropertyName("eightTonnageChange")]
        public ChangeJson EightTonnageChange { get; set; } = new ChangeJson();

        private const string FourCountryApportionmentPercentage = "4 Country Apportionment %s";
        private const string EightTonnageChangeHeader = "8 Tonnage Change";

        public static CalcResultParametersOtherJson From(CalcResultParameterOtherCost otherCost)
        {
            return new CalcResultParametersOtherJson
            {
                ThreeSAOperatingCost = CountryAmountJson.From(otherCost.SaOperatingCost),
                FourDataPreparationCharge = CountryAmountJson.From(otherCost.LaDataPrepCharge),
                FourCountryApportionmentPercentages = CountryAmountJson.From(otherCost.CountryApportionment),
                FiveSchemeSetupCost = CountryAmountJson.From(otherCost.SchemeSetupCost),
                SixBadDebtProvision = new PercentageJson
                {
                    Percentage = $"{otherCost.BadDebtValue:0.00}%"
                },
                SevenMateriality = ChangeJson.From(otherCost.MaterialityIncrease, otherCost.MaterialityDecrease),
                EightTonnageChange = ChangeJson.From(otherCost.TonnageChangeIncrease, otherCost.TonnageChangeDecrease),
            };
        }
    }

    public class CountryAmountJson
    {
        [JsonPropertyName("england")]
        public string England { get; set; } = string.Empty;

        [JsonPropertyName("wales")]
        public string Wales { get; set; } = string.Empty;

        [JsonPropertyName("scotland")]
        public string Scotland { get; set; } = string.Empty;

        [JsonPropertyName("northernIreland")]
        public string NorthernIreland { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public string Total { get; set; } = string.Empty;

        public static CountryAmountJson From(CalcResultParameterOtherCostDetail? costDetail)
        {
            if (costDetail == null) return new CountryAmountJson();

            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            return new CountryAmountJson
            {
                England = costDetail.England.ToString("C", culture),
                Wales = costDetail.Wales.ToString("C", culture),
                Scotland = costDetail.Scotland.ToString("C", culture),
                NorthernIreland = costDetail.NorthernIreland.ToString("C", culture),
                Total = costDetail.Total.ToString("C", culture),
            };
        }
    }

    public class PercentageJson
    {
        [JsonPropertyName("percentage")]
        public string Percentage { get; set; } = string.Empty;
    }

    public class ChangeJson
    {
        [JsonPropertyName("increase")]
        public ChangeDetailJson Increase { get; set; } = new ChangeDetailJson();

        [JsonPropertyName("decrease")]
        public ChangeDetailJson Decrease { get; set; } = new ChangeDetailJson();

        public static ChangeJson From(Materiality increase, Materiality decrease)
        {
            return new ChangeJson
            {
                Increase = ChangeDetailJson.From(increase),
                Decrease = ChangeDetailJson.From(decrease),
            };
        }
    }

    public class ChangeDetailJson
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("percentage")]
        public string Percentage { get; set; } = string.Empty;

        public static ChangeDetailJson From(Materiality materiality)
        {
            return new ChangeDetailJson
            {
                Amount     = CurrencyConverterUtils.ConvertToCurrency(materiality.Amount),
                Percentage = $"{materiality.Percentage:0.00}%"
            };
        }
    }
}
