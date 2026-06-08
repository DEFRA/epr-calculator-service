using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section2aComms2aExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision",
            "Bad Debt Provision for 2a",
            "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision",
            "England Total with Bad Debt provision",
            "Wales Total with Bad Debt provision",
            "Scotland Total with Bad Debt provision",
            "Northern Ireland Total with Bad Debt provision"
        ];
    }

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("2a Fee for Comms Costs - by Material w/o Bad Debt provision"));
        csvContent.Append(CsvSanitiser.SanitiseData("Bad Debt Provision"));
        csvContent.Append(CsvSanitiser.SanitiseData("2a Fee for Comms Costs - by Material with Bad Debt provision"));
        csvContent.Append(',', count - 3);
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(resultSummary.CommsCostsSection2a.FeeWithoutBadDebtProvision   , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(resultSummary.CommsCostsSection2a.BadDebtProvision             , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(resultSummary.CommsCostsSection2a.FeeWithBadDebtProvision.Total, DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(',', count - 3);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        var costs = producer.CommsCostsSection2a;
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.FeeWithoutBadDebtProvision             , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.BadDebtProvision                       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.FeeWithBadDebtProvision.Total          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.FeeWithBadDebtProvision.England        , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.FeeWithBadDebtProvision.Wales          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.FeeWithBadDebtProvision.Scotland       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.FeeWithBadDebtProvision.NorthernIreland, DecimalPlaces.Two, null, isCurrency: true));
    }
}
