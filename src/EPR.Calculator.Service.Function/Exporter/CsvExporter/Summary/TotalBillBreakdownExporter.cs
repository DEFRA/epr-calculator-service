using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class TotalBillBreakdownExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "Total Producer Bill (1+2a+2b+2c+3+4+5) w/o Bad Debt Provision",
            "Bad Debt Provision for Total Producer Bill",
            "Total Producer Bill (1+2a+2b+2c+3+4+5) with Bad Debt Provision",
            "England Total with Bad Debt provision",
            "Wales Total with Bad Debt provision",
            "Scotland Total with Bad Debt provision",
            "Northern Ireland Total with Bad Debt provision"
        ];
    }

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("Total Producer Bill Breakdown"));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        var costs = producer.TotalProducerBillBreakdownCosts;
        csvContent.Append(CsvSanitiser.SanitiseData(costs.FeeWithoutBadDebtProvision             , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.BadDebtProvision                       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.FeeWithBadDebtProvision.Total          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.FeeWithBadDebtProvision.England        , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.FeeWithBadDebtProvision.Wales          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.FeeWithBadDebtProvision.Scotland       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs.FeeWithBadDebtProvision.NorthernIreland, DecimalPlaces.Two, null, isCurrency: true));
    }
}
