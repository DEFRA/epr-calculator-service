using System.Text;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using System.Globalization;
using NetTopologySuite.Operation.Buffer;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts
{
    public interface ICalcResultParameterOtherCostExporter
    {
        void Export(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);
    }

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
            csvContent.AppendLine(CsvSanitiser.SanitiseData("Parameters - Other"));
            SaOperatingCosts(otherCost, csvContent);

            csvContent.AppendLine();

            LaDataPrepCosts(otherCost, csvContent);
            CountryApportionment(otherCost, csvContent);

            csvContent.AppendLine();
            SchemeSetupCost(otherCost, csvContent);

            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("6 Bad Debt Provision"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.BadDebtValue, Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isPercentage: true));
            csvContent.AppendLine();

            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("7 Materiality"));
            csvContent.Append(CsvSanitiser.SanitiseData("Amount £s"));
            csvContent.Append(CsvSanitiser.SanitiseData("%"));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Increase"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.MaterialityIncrease.Amount, Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.MaterialityIncrease.Percentage));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Decrease"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.MaterialityDecrease.Amount, Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.MaterialityDecrease.Percentage));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("8 Tonnage Change"));
            csvContent.Append(CsvSanitiser.SanitiseData("Amount £s"));
            csvContent.Append(CsvSanitiser.SanitiseData("%"));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Increase"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.TonnageChangeIncrease.Amount, Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.TonnageChangeIncrease.Percentage));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Decrease"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.TonnageChangeDecrease.Amount, Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.TonnageChangeDecrease.Percentage));
        }

        public void SchemeSetupCost(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("5 Scheme set up cost Yearly Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.England        , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.Wales          , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.Scotland       , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.NorthernIreland, Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.Total          , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }

        public void LaDataPrepCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("4 LA Data Prep Charge"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.England        , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.Wales          , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.Scotland       , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.NorthernIreland, Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.Total          , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }

        public void CountryApportionment(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("4 Country Apportionment %s"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.England        , Enums.DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.Wales          , Enums.DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.Scotland       , Enums.DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.NorthernIreland, Enums.DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.Total          , Enums.DecimalPlaces.Eight, null, isPercentage: true));
            csvContent.AppendLine();
        }

        public void SaOperatingCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData((string)null));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("3 SA Operating Costs"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.England        , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.Wales          , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.Scotland       , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.NorthernIreland, Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.Total          , Enums.DecimalPlaces.Two, Enums.DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }
    }
}
