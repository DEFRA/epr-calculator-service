using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummary
    {
        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; } = new List<CalcResultSummaryProducerDisposalFees>();

        public required CalcResultSummaryProducerDisposalFees OverallTotal { get; set; } = new CalcResultSummaryProducerDisposalFees { CalculatorRunId = 0, ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty };
    }
}
