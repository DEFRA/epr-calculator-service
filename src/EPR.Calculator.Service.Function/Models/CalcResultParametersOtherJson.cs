using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultParametersOtherJson
    {
        [JsonProperty(PropertyName = "threeSAOperatingCost")]
        public CountryAmountJson ThreeSAOperatingCost { get; set; } = new CountryAmountJson();

        [JsonProperty(PropertyName = "fourDataPreparationCharge")]
        public CountryAmountJson FourLADataPrepCosts { get; set; } = new CountryAmountJson();

        [JsonProperty(PropertyName = "fourCountryApportionmentPercentages")]
        public CountryAmountJson FourCountryApportionmentPercentages { get; set; } = new CountryAmountJson();

        [JsonProperty(PropertyName = "fiveSchemeSetupCost")]
        public CountryAmountJson FiveSchemeSetupYearlyCost { get; set; } = new CountryAmountJson();

        [JsonProperty(PropertyName = "sixBadDebtProvision")]
        public PercentageJson SixBadDebtProvision { get; set; } = new PercentageJson();

        [JsonProperty(PropertyName = "sevenMateriality")]
        public ChangeJson SevenMateriality { get; set; } = new ChangeJson();

        [JsonProperty(PropertyName = "eightTonnageChange")]
        public ChangeJson EightTonnageChange { get; set; } = new ChangeJson();
    }

    public class CountryAmountJson
    {
        [JsonProperty(PropertyName = "england")]
        public string England { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "wales")]
        public string Wales { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "scotland")]
        public string Scotland { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "northernIreland")]
        public string NorthernIreland { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; } = string.Empty;
    }

    public class PercentageJson
    {
        [JsonProperty(PropertyName = "percentage")]
        public string Percentage { get; set; } = string.Empty;
    }

    public class ChangeJson
    {
        [JsonProperty(PropertyName = "increase")]
        public ChangeDetailJson Increase { get; set; } = new ChangeDetailJson();

        [JsonProperty(PropertyName = "decrease")]
        public ChangeDetailJson Decrease { get; set; } = new ChangeDetailJson();
    }

    public class ChangeDetailJson
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "percentage")]
        public string Percentage { get; set; } = string.Empty;
    }
}