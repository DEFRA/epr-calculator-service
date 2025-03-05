namespace EPR.Calculator.Service.Function.Exporter.CommsCost
{
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

    /// <summary>
    /// Provides functionality to export communication cost details to a CSV format.
    /// </summary>
    public class CommsCostExporter : ICommsCostExporter
    {
        /// <summary>
        /// Exports the communication cost details to the provided StringBuilder in CSV format.
        /// </summary>
        /// <param name="communicationCost">The communication cost details to export.</param>
        /// <param name="csvContent">The csv contenst.</param>
        public void Export(CalcResultCommsCost communicationCost, StringBuilder csvContent)
            {
                return csvContent;
            }

            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.AppendLine(communicationCost.Name);

            var apportionment = communicationCost.CalcResultCommsCostOnePlusFourApportionment;

            foreach (var onePlusFourApportionment in apportionment)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.England));
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.NorthernIreland));
                csvContent.AppendLine(CsvSanitiser.SanitiseData(onePlusFourApportionment.Total));
            }

            csvContent.AppendLine();
            var commCostByMaterials = communicationCost.CalcResultCommsCostCommsCostByMaterial;

            foreach (var commCostByMaterial in commCostByMaterials)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.England));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Total));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ProducerReportedHouseholdPackagingWasteTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ReportedPublicBinTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.HouseholdDrinksContainers));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.LateReportingTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ProducerReportedHouseholdPlusLateReportingTonnage));

                if (commCostByMaterial.Total == CommonConstants.Total || string.IsNullOrWhiteSpace(commCostByMaterial.CommsCostByMaterialPricePerTonne))
                {
                    csvContent.AppendLine(CsvSanitiser.SanitiseData(commCostByMaterial.CommsCostByMaterialPricePerTonne));
                }
                else
                {
                    csvContent.AppendLine(CsvSanitiser.SanitiseData(commCostByMaterial.CommsCostByMaterialPricePerTonne, DecimalPlaces.Four, null, true, false));
                }
            }

            csvContent.AppendLine();
            var countryList = communicationCost.CommsCostByCountry;
            foreach (var country in countryList)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(country.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(country.England));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(country.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Total));
                csvContent.AppendLine();
            }

            return csvContent;
        }
    }
}