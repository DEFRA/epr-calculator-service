using System.Text;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using System.Globalization;
using NetTopologySuite.Operation.Buffer;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts
{
    public class CalcResultParameterOtherCostExporter : ICalcResultParameterOtherCostExporter
    {
        private CultureInfo culture = InitCulture();

        private static CultureInfo InitCulture()
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            return culture;
        }

        public void Export(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.AppendLine(otherCost.Name);
            SaOpertingCosts(otherCost, csvContent);

            csvContent.AppendLine();

            LaDataPrepCosts(otherCost, csvContent);

            csvContent.AppendLine();
            SchemeSetupCost(otherCost, csvContent);

            csvContent.AppendLine();
            csvContent.Append($"{CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Key)}");
            csvContent.AppendLine($"{CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Value)}");

            csvContent.AppendLine();
            Materiality(otherCost, csvContent);
        }

        public void Materiality(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            var materiality = otherCost.Materiality;
            foreach (var material in materiality)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(material.SevenMateriality)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(material.Amount)}");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(material.Percentage)}");
            }
        }

        public void SchemeSetupCost(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            var schemeCost = otherCost.SchemeSetupCost;
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Name)}");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.England.ToString("C", culture))}");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Wales.ToString("C", culture))}");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Scotland.ToString("C", culture))}");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.NorthernIreland.ToString("C", culture))}");
            csvContent.AppendLine($"{CsvSanitiser.SanitiseData(schemeCost.Total.ToString("C", culture))}");
        }

        public void LaDataPrepCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            var laDataPreps = otherCost.Details.OrderBy(x => x.OrderId);

            foreach (var laDataPrep in laDataPreps)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Name)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.England)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Wales)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Scotland)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.NorthernIreland)}");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(laDataPrep.Total)}");
            }
        }

        public void SaOpertingCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            var saOperatingCosts = otherCost.SaOperatingCost.OrderBy(x => x.OrderId);

            foreach (var saOperatingCost in saOperatingCosts)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Name)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.England)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Wales)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Scotland)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.NorthernIreland)}");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(saOperatingCost.Total)}");
            }
        }
    }
}
