using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class OnePlus2a2b2cExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => OnePlus2A2B2CProducer.GetHeaders();

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(OnePlus2A2B2CHeaders.TotalWithBadDebtProvision));
        csvContent.Append(',', count - 1);
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision, 2)}"));
        csvContent.Append(',', count - 1);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, DecimalPlaces.Eight, DecimalFormats.F8, false, true));
    }
}
