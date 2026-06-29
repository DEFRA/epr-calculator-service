using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section1DisposalFeeExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "1 Total Producer Disposal Fee w/o Bad Debt Provision",
            "Bad Debt Provision",
            "1 Total Producer Disposal Fee with Bad Debt Provision",
            "England Total",
            "Wales Total",
            "Scotland Total",
            "Northern Ireland Total",
            "Tonnage Change Count",
            "Tonnage Change Advice"
        ];
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("Disposal Fee Summary"));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LADisposalCostsSection1.FeeWithoutBadDebtProvision             , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LADisposalCostsSection1.BadDebtProvision                       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Total          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.England        , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Wales          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Scotland       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.NorthernIreland, DecimalPlaces.Two, null, isCurrency: true));
        AppendCsvValue(csvContent, producer.TonnageChangeCount, producer.IsOverallTotal);
        AppendCsvValue(csvContent, producer.TonnageChangeAdvice, producer.IsOverallTotal);
    }

    private static void AppendCsvValue(
        StringBuilder csvContent,
        string? value,
        bool isOverallTotalRow
    )
    {
        if (value == null && !isOverallTotalRow)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
        }
        else
        {
            csvContent.Append(CsvSanitiser.SanitiseData(value));
        }
    }
}
