using System.Text;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class ProducerIdentityExporter : ICalcResultSummaryPartExporter
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
        string YesOrNo(bool isValueSet) {
            if (producer.IsOverallRow)
                return string.Empty;
            
            return isValueSet ? CommonConstants.Yes : CommonConstants.No;
        }
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId == 0 ? string.Empty : producer.ProducerId.ToString()));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
        csvContent.Append(CsvSanitiser.SanitiseData(YesOrNo(producer.IsProducerScaledup)));
        csvContent.Append(CsvSanitiser.SanitiseData(YesOrNo(producer.IsPartialObligation)));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.StatusCode));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.JoinerDate));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LeaverDate));
    }
}
