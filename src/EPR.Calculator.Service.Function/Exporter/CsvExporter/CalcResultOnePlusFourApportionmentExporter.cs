using System.Text;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using System.Globalization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface ICalcResultOnePlusFourApportionmentExporter
    {
        void Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent);
    }

    public class CalcResultOnePlusFourApportionmentExporter : ICalcResultOnePlusFourApportionmentExporter
    {
        public void Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData("1 + 4 Apportionment %s"));

            AppendHeaders(csvContent);

            var lapcapDataDetails = calcResult1Plus4Apportionment.CalcResultOnePlusFourApportionmentDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Name));
                if (lapcapData.Name == "1 + 4 Apportionment %s")
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(100 * lapcapData.EnglandTotal        , DecimalPlaces.Eight, null, isPercentage: true));
                    csvContent.Append(CsvSanitiser.SanitiseData(100 * lapcapData.WalesTotal          , DecimalPlaces.Eight, null, isPercentage: true));
                    csvContent.Append(CsvSanitiser.SanitiseData(100 * lapcapData.ScotlandTotal       , DecimalPlaces.Eight, null, isPercentage: true));
                    csvContent.Append(CsvSanitiser.SanitiseData(100 * lapcapData.NorthernIrelandTotal, DecimalPlaces.Eight, null, isPercentage: true));
                    csvContent.Append(CsvSanitiser.SanitiseData(100                                  , DecimalPlaces.Eight, null, isPercentage: true));
                }
                else
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.EnglandTotal        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                    csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.WalesTotal          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                    csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ScotlandTotal       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                    csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandTotal, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                    csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Total               , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
                }
                csvContent.AppendLine();
            }
        }

        private void AppendHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(""));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.AppendLine();
        }
    }
}
