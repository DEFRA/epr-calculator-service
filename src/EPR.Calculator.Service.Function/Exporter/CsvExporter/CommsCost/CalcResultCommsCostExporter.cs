using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Builder.CommsCost;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost
{
    public interface ICalcResultCommsCostExporter
    {
        void Export(CalcResultCommsCost communicationCost, StringBuilder csvContent);
    }

    /// <summary>
    /// Provides functionality to export communication cost details to a CSV format.
    /// </summary>
    public class CalcResultCommsCostExporter : ICalcResultCommsCostExporter
    {
        /// <summary>
        /// Exports the communication cost details to the provided StringBuilder in CSV format.
        /// </summary>
        /// <param name="communicationCost">The communication cost details to export.</param>
        /// <param name="csvContent">The csv contenst.</param>
        public void Export(CalcResultCommsCost communicationCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.AppendLine(CsvSanitiser.SanitiseData("Parameters - Comms Costs"));

            var apportionments = communicationCost.CalcResultCommsCostOnePlusFourApportionment;

            AppendHeaderApportionmentHeaders(csvContent);
            // TODO is this only a List since it previously contained Headers
            foreach (var onePlusFourApportionment in apportionments)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Name));
                csvContent.Append(CsvSanitiser.SanitiseData($"{onePlusFourApportionment.England : 0.00000000}%"));
                csvContent.Append(CsvSanitiser.SanitiseData($"{onePlusFourApportionment.Wales : 0.00000000}%"));
                csvContent.Append(CsvSanitiser.SanitiseData($"{onePlusFourApportionment.Scotland : 0.00000000}%"));
                csvContent.Append(CsvSanitiser.SanitiseData($"{onePlusFourApportionment.NorthernIreland : 0.00000000}%"));
                csvContent.AppendLine(CsvSanitiser.SanitiseData($"{onePlusFourApportionment.Total : 0.00000000}%"));
            }

            csvContent.AppendLine();
            var commCostByMaterials = communicationCost.CalcResultCommsCostCommsCostByMaterial;
            AppendHeader(csvContent);

            foreach (var commCostByMaterial in commCostByMaterials)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.England, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Wales, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Scotland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Total, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ProducerReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.HouseholdDrinksContainers, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.LateReportingTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ProducerReportedTotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (commCostByMaterial.CommsCostByMaterialPricePerTonne == null)
                {
                    csvContent.AppendLine(CsvSanitiser.SanitiseData(commCostByMaterial.CommsCostByMaterialPricePerTonne));
                }
                else
                {
                    csvContent.AppendLine(CsvSanitiser.SanitiseData(commCostByMaterial.CommsCostByMaterialPricePerTonne, DecimalPlaces.Four, null, isCurrency: true));
                }
            }

            csvContent.AppendLine();
            var countryList = communicationCost.CommsCostByCountry;
            foreach (var country in countryList)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(country.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(country.England, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Wales, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Scotland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(country.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Total, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.AppendLine();
            }
        }

        private void AppendHeaderApportionmentHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData((string) null));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.AppendLine();
        }

        private void AppendHeader(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("2a Comms Costs - by Material"));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.Append(CsvSanitiser.SanitiseData("Producer Household Packaging Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Late Reporting Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Public Bin Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Household Drinks Containers Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Comms Cost - by Material Price Per Tonne"));
            csvContent.AppendLine();
        }
    }
}
