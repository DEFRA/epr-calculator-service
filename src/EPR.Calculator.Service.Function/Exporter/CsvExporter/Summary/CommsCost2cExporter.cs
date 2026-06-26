using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class CommsCost2cExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return [
            "2c Total Producer Fee for Comms Costs - by Country In proportion to Producer Tonnage w/o Bad Debt provision",
            "Bad Debt Provision for 2c",
            "2c Total Producer Fee for Comms Costs - by Country In proportion to Producer Tonnage with Bad Debt provision",
            "England Total with Bad Debt provision",
            "Wales Total with Bad Debt provision",
            "Scotland Total with Bad Debt provision",
            "Northern Ireland Total with Bad Debt provision"
        ];
    }

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("2c Comms Costs - by Country w/o Bad Debt provision"));
        csvContent.Append(CsvSanitiser.SanitiseData("Bad Debt provision"));
        csvContent.Append(CsvSanitiser.SanitiseData("2c Comms Costs - by Country with Bad Debt provision"));
        csvContent.Append(',', count - 3);
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(resultSummary.CommsCostsSection2c.FeeWithoutBadDebtProvision   , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(resultSummary.CommsCostsSection2c.BadDebtProvision             , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(resultSummary.CommsCostsSection2c.FeeWithBadDebtProvision.Total, DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(',', count - 3);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.CommsCostsSection2c.FeeWithoutBadDebtProvision             , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.CommsCostsSection2c.BadDebtProvision                       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.CommsCostsSection2c.FeeWithBadDebtProvision.Total          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.CommsCostsSection2c.FeeWithBadDebtProvision.England        , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.CommsCostsSection2c.FeeWithBadDebtProvision.Wales          , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.CommsCostsSection2c.FeeWithBadDebtProvision.Scotland       , DecimalPlaces.Two, null, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.CommsCostsSection2c.FeeWithBadDebtProvision.NorthernIreland, DecimalPlaces.Two, null, isCurrency: true));
    }
}
