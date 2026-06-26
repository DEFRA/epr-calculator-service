using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummary
    {
        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; } = new List<CalcResultSummaryProducerDisposalFees>();

        public CalcResultSummaryProducerDisposalFees OverallTotal { get; set; }

        public decimal TotalOnePlus2A2B2CFeeWithBadDebtProvision =>
            OverallTotal.LADisposalCostsSection1.FeeWithBadDebtProvision.Total
            + OverallTotal.CommsCostsSection2a.FeeWithBadDebtProvision.Total
            + OverallTotal.CommsCostsSection2b.FeeWithBadDebtProvision.Total
            + OverallTotal.CommsCostsSection2c.FeeWithBadDebtProvision.Total;
    }
}
