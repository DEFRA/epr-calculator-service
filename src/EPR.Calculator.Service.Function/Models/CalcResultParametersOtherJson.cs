using System.Text.Json.Serialization;

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
    }

    public class ChangeDetailJson
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("percentage")]
        public string Percentage { get; set; } = string.Empty;
    }
}