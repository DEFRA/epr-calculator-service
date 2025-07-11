﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class ParametersOtherMapper : IParametersOtherMapper
    {
        public const string FourCountryApportionmentPercentage = "4 Country Apportionment %s";
        public const string EightTonnageChangeHeader = "8 Tonnage Change";

        public CalcResultParametersOtherJson Map(CalcResultParameterOtherCost otherCost)
        {
            var apportionmentDetail = otherCost.Details
                .FirstOrDefault(d => d.Name == FourCountryApportionmentPercentage);

            var (materiality, tonnageChange) = SplitMaterialitySections(otherCost.Materiality);

            return new CalcResultParametersOtherJson
            {
                ThreeSAOperatingCost = MapCountryAmount(otherCost.SaOperatingCost
                    .Where(sa => decimal.TryParse(sa.England, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out _)) // Filter out the header line.
                    .OrderBy(sa => sa.OrderId)
                    .FirstOrDefault()),
                FourDataPreparationCharge = MapCountryAmount(otherCost.Details.OrderBy(sa => sa.OrderId).FirstOrDefault()),
                FourCountryApportionmentPercentages = MapCountryAmount(apportionmentDetail),
                FiveSchemeSetupCost = MapCountryAmount(otherCost.SchemeSetupCost),
                SixBadDebtProvision = new PercentageJson
                {
                    Percentage = otherCost.BadDebtProvision.Value
                },
                SevenMateriality = MapChangeSection(materiality),
                EightTonnageChange = MapChangeSection(tonnageChange),
            };
        }

        private static (IEnumerable<CalcResultMateriality> materiality, IEnumerable<CalcResultMateriality> tonnageChange)
            SplitMaterialitySections(IEnumerable<CalcResultMateriality> materialities)
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

        private static ChangeJson MapChangeSection(IEnumerable<CalcResultMateriality> materialities)
        {
            if (!materialities.Any()) return new ChangeJson();

            var increase = materialities.FirstOrDefault(m => m.SevenMateriality == "Increase");
            var decrease = materialities.FirstOrDefault(m => m.SevenMateriality == "Decrease");

            return new ChangeJson
            {
                Increase = MapChangeDetail(increase),
                Decrease = MapChangeDetail(decrease),
            };
        }

        private static CountryAmountJson MapCountryAmount(CalcResultParameterOtherCostDetail? costDetail)
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

        private static ChangeDetailJson MapChangeDetail(CalcResultMateriality? materiality)
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