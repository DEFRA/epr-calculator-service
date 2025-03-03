using EPR.Calculator.API.Utils;
using EPR.Calculator.Service.Function.Models;
using System.Linq;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter
{
    public class CalcResultParameterOtherCostExporter
    {
        public static StringBuilder ExportOtherCost(CalcResultParameterOtherCost otherCost)
        {
            var csvContent = new StringBuilder();

            if (otherCost == null)
            {
                return csvContent;
            }

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

            return csvContent;
        }

        public static StringBuilder Materiality(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            var materiality = otherCost.Materiality;
            foreach (var material in materiality)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(material.SevenMateriality)}");
                csvContent.Append($"{CsvSanitiser.SanitiseData(material.Amount)}");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(material.Percentage)}");
            }

            return csvContent;
        }

        public static StringBuilder SchemeSetupCost(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            var schemeCost = otherCost.SchemeSetupCost;
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Name)}");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.England)}");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Wales)}");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Scotland)}");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.NorthernIreland)}");
            csvContent.AppendLine($"{CsvSanitiser.SanitiseData(schemeCost.Total)}");

            return csvContent;
        }

        public static StringBuilder LaDataPrepCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
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

            return csvContent;
        }

        public static StringBuilder SaOpertingCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
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

            return csvContent;
        }
    }
}
