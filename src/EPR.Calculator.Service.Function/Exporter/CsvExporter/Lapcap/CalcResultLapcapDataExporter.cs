using System.Text;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using System.Globalization;
using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap
{
    public interface ICalcResultLapcapDataExporter
    {
        void Export(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent);
    }

    public class CalcResultLapcapDataExporter : ICalcResultLapcapDataExporter
    {
        public void Export(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData("LAPCAP Data"));

            AppendHeaders(csvContent);
            var lapcapDataDetails = calcResultLapcapData.CalcResultLapcapDataDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.EnglandCost        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.WalesCost          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ScotlandCost       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandCost, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.TotalCost          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                csvContent.AppendLine();
            }

            csvContent.Append($"{CsvSanitiser.SanitiseData("1 Country Apportionment %s")}");
            csvContent.Append(CsvSanitiser.SanitiseData(100 * calcResultLapcapData.CountryApportionment.England        , DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(100 * calcResultLapcapData.CountryApportionment.Wales          , DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(100 * calcResultLapcapData.CountryApportionment.Scotland       , DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(100 * calcResultLapcapData.CountryApportionment.NorthernIreland, DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(100                                                            , DecimalPlaces.Eight, null, isPercentage: true));

            csvContent.AppendLine();
        }

        private void AppendHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("Material"));
            csvContent.Append(CsvSanitiser.SanitiseData("England LA Disposal Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales LA Disposal Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland LA Disposal Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland LA Disposal Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData("1 LA Disposal Cost Total"));
            csvContent.AppendLine();
        }
    }
}
