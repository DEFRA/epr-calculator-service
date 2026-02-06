using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private const string FourCountryApportionmentPercentage = "4 Country Apportionment %s";
        private const string EightTonnageChangeHeader = "8 Tonnage Change";

        public static CalcResultParametersOtherJson From(CalcResultParameterOtherCost otherCost)
        {
            (IEnumerable<CalcResultMateriality> materiality, IEnumerable<CalcResultMateriality> tonnageChange) SplitMaterialitySections(IEnumerable<CalcResultMateriality> materialities)
            {
                var allItems = materialities.ToList();

                var tonnageHeader = EightTonnageChangeHeader;
                int tonnageHeaderIndex = allItems.FindIndex(m => m.SevenMateriality == tonnageHeader);

                if (tonnageHeaderIndex == -1)
                {
                    return (allItems, Enumerable.Empty<CalcResultMateriality>());
                }

                var materialitySection = allItems.Take(tonnageHeaderIndex);
                var tonnageChangeSection = allItems.Skip(tonnageHeaderIndex + 1);

                return (materialitySection, tonnageChangeSection);
            }

            var apportionmentDetail = otherCost.Details.FirstOrDefault(d => d.Name == FourCountryApportionmentPercentage);

            var (materiality, tonnageChange) = SplitMaterialitySections(otherCost.Materiality);

            return new CalcResultParametersOtherJson
            {
                ThreeSAOperatingCost = CountryAmountJson.From(otherCost.SaOperatingCost
                    .Where(sa => decimal.TryParse(sa.England, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out _)) // Filter out the header line.
                    .OrderBy(sa => sa.OrderId)
                    .FirstOrDefault()),
                FourDataPreparationCharge = CountryAmountJson.From(otherCost.Details.OrderBy(sa => sa.OrderId).FirstOrDefault()),
                FourCountryApportionmentPercentages = CountryAmountJson.From(apportionmentDetail),
                FiveSchemeSetupCost = CountryAmountJson.From(otherCost.SchemeSetupCost),
                SixBadDebtProvision = new PercentageJson
                {
                    Percentage = otherCost.BadDebtProvision.Value
                },
                SevenMateriality = ChangeJson.From(materiality),
                EightTonnageChange = ChangeJson.From(tonnageChange),
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
            return new CountryAmountJson
            {
                England = costDetail.England,
                Wales = costDetail.Wales,
                Scotland = costDetail.Scotland,
                NorthernIreland = costDetail.NorthernIreland,
                Total = costDetail.Total,
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

        public static ChangeJson From(IEnumerable<CalcResultMateriality> materialities)
        {
            if (!materialities.Any()) return new ChangeJson();

            var increase = materialities.FirstOrDefault(m => m.SevenMateriality == "Increase");
            var decrease = materialities.FirstOrDefault(m => m.SevenMateriality == "Decrease");

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

        public static ChangeDetailJson From(CalcResultMateriality? materiality)
        {
            if (materiality == null) return new ChangeDetailJson();
            return new ChangeDetailJson
            {
                Amount = materiality.Amount,
                Percentage = materiality.Percentage,
            };
        }
    }
}