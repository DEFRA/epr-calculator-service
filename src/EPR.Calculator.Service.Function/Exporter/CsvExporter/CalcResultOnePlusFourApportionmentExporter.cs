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

            csvContent.Append(CsvSanitiser.SanitiseData("1 Fee for LA Disposal Costs"));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LaDisposalCost.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LaDisposalCost.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LaDisposalCost.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LaDisposalCost.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LaDisposalCost.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("4 LA Data Prep Charge"));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LADataPrepCharge.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LADataPrepCharge.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LADataPrepCharge.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LADataPrepCharge.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.LADataPrepCharge.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("Total of 1 + 4"));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.TotalOnePlusFour.EnglandTotal        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.TotalOnePlusFour.WalesTotal          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.TotalOnePlusFour.ScotlandTotal       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.TotalOnePlusFour.NorthernIrelandTotal, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.TotalOnePlusFour.Total               , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("1 + 4 Apportionment %s"));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.OnePlusFourApportionment.England        , DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.OnePlusFourApportionment.Wales          , DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.OnePlusFourApportionment.Scotland       , DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResult1Plus4Apportionment.OnePlusFourApportionment.NorthernIreland, DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(100                                                                   , DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.AppendLine();
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
