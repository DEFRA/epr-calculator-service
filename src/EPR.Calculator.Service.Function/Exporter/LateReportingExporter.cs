namespace EPR.Calculator.Service.Function.Exporter
{
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Models;

    /// <summary>
    /// Exports the Late Reporting Tonnage data to a string to be added to the results file.
    /// </summary>
    /// <param name="calcResultLateReportingData">The data to export.</param>
    public class LateReportingExporter()
    {
        public string PrepareData(CalcResultLateReportingTonnage? calcResultLateReportingData)
        {
            var csvContent = new StringBuilder();
            if (calcResultLateReportingData is null)
            {
                return string.Empty;
            }

            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLateReportingData.Name);
            csvContent.Append($"{calcResultLateReportingData.MaterialHeading},");
            csvContent.Append(calcResultLateReportingData.TonnageHeading);
            csvContent.AppendLine();

            foreach (var lateReportingData in calcResultLateReportingData.CalcResultLateReportingTonnageDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lateReportingData.Name)},");
                csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.TotalLateReportingTonnage));
                csvContent.AppendLine();
            }

            return csvContent.ToString();
        }
    }
}
