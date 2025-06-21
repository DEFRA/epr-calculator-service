using EPR.Calculator.Service.Function.Models;
using System.Collections.Generic;
using System.Linq;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class ParametersOtherMapper : IParametersOtherMapper
    {
        public CalcResultParametersOtherJson Map(CalcResultParameterOtherCost otherCost)
        {
            // Find the apportionment detail by name
            var apportionmentDetail = otherCost.Details
                .FirstOrDefault(d => d.Name == "4 Country Apportionment %s");

            // Extract materiality and tonnage change sections
            var (materiality, tonnageChange) = SplitMaterialitySections(otherCost.Materiality);

            return new CalcResultParametersOtherJson
            {
                ParametersOther = new ParametersOtherDetailsJson
                {
                    ThreeSAOperatingCost = MapCountryAmount(otherCost.SaOperatingCost.OrderBy(sa => sa.OrderId)?.FirstOrDefault()),
                    FourLADataPrepCosts = MapCountryAmount(otherCost.Details.OrderBy(sa => sa.OrderId)?.FirstOrDefault()),
                    FourCountryApportionmentPercentages = MapCountryAmount(apportionmentDetail),
                    FiveSchemeSetupYearlyCost = MapCountryAmount(otherCost.SchemeSetupCost),
                    SixBadDebtProvision = new PercentageJson
                    {
                        Percentage = otherCost.BadDebtProvision.Value
                    },
                    SevenMateriality = MapChangeSection(materiality),
                    EightTonnageChange = MapChangeSection(tonnageChange),
                }
            };
        }

        // Helper to split the materiality list into two sections
        private static (IEnumerable<CalcResultMateriality> materiality, IEnumerable<CalcResultMateriality> tonnageChange)
            SplitMaterialitySections(IEnumerable<CalcResultMateriality> materialities)
        {
            var list = materialities?.ToList() ?? new List<CalcResultMateriality>();
            // Find the index of the "8 Tonnage Change" header
            var tonnageHeaderIndex = list.FindIndex(m => m.SevenMateriality == "8 Tonnage Change");
            if (tonnageHeaderIndex >= 0)
            {
                // Materiality is before the header, tonnage change is after
                return (
                    list.Take(tonnageHeaderIndex),
                    list.Skip(tonnageHeaderIndex + 1)
                );
            }
            // If not found, treat all as materiality, none as tonnage change
            return (list, Enumerable.Empty<CalcResultMateriality>());
        }

        // Map a section (increase/decrease) to ChangeJson
        private static ChangeJson MapChangeSection(IEnumerable<CalcResultMateriality> section)
        {
            if (section == null) return new ChangeJson();

            var increase = section.FirstOrDefault(m => m.SevenMateriality == "Increase");
            var decrease = section.FirstOrDefault(m => m.SevenMateriality == "Decrease");

            return new ChangeJson
            {
                Increase = MapChangeDetail(increase),
                Decrease = MapChangeDetail(decrease),
            };
        }

        private static CountryAmountJson MapCountryAmount(CalcResultParameterOtherCostDetail? source)
        {
            if (source == null) return new CountryAmountJson();
            return new CountryAmountJson
            {
                England = source.England,
                Wales = source.Wales,
                Scotland = source.Scotland,
                NorthernIreland = source.NorthernIreland,
                Total = source.Total,
            };
        }

        private static ChangeDetailJson MapChangeDetail(CalcResultMateriality? source)
        {
            if (source == null) return new ChangeDetailJson();
            return new ChangeDetailJson
            {
                Amount = source.Amount,
                Percentage = source.Percentage,
            };
        }
    }
}