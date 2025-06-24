using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class DisposalFeeSummary1Mapper : IDisposalFeeSummary1Mapper
    {
        public DisposalFeeSummary1 Map(CalcResultSummaryProducerDisposalFees summary)
        {
            return new DisposalFeeSummary1
            {
                TotalProducerDisposalFeeWithoutBadDebtProvision = summary.TotalProducerDisposalFee,
                BadDebtProvision = summary.BadDebtProvision,
                TotalProducerDisposalFeeWithBadDebtProvision = summary.TotalProducerDisposalFeeWithBadDebtProvision,
                EnglandTotal = summary.EnglandTotal,
                WalesTotal = summary.WalesTotal,
                ScotlandTotal = summary.ScotlandTotal,
                NorthernIrelandTotal = summary.NorthernIrelandTotal,
                TonnageChangeCount = summary.TonnageChangeCount,
                TonnageChangeAdvice = summary.TonnageChangeAdvice,
            };
        }
    }
}
