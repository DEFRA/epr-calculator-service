using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class ProducerIdentityExporter(
    IReadOnlyList<int> scaledupProducerIds,
    IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds
) : ICalcResultSummaryPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return
        [
            "Producer ID",
            "Subsidiary ID",
            "Producer / Subsidiary Name",
            "Trading Name",
            "Level",
            "Scaled-up tonnages?",
            "Partial Calculation?",
            "Registration Status Code",
            "Joiners Date",
            "Leavers Date"
        ];
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        string YesOrNo(bool isValueSet) =>
            producer.IsOverallTotal ? string.Empty : (isValueSet ? CommonConstants.Yes : CommonConstants.No);

        var isScaledup = scaledupProducerIds.Contains(producer.ProducerId);
        var isPartialObligation = producer.Level == "1"
            ? partialProducerSubsidiaryIds.Any(p => p.Item1 == producer.ProducerId)
            : partialProducerSubsidiaryIds.Contains((producer.ProducerId, producer.SubsidiaryId));

        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId == 0 ? string.Empty : producer.ProducerId.ToString()));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
        csvContent.Append(CsvSanitiser.SanitiseData(YesOrNo(isScaledup)));
        csvContent.Append(CsvSanitiser.SanitiseData(YesOrNo(isPartialObligation)));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.StatusCode));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.JoinerDate));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LeaverDate));
    }
}
