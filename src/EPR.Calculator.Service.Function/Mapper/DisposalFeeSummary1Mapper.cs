using EPR.Calculator.Service.Common.Utils;
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
                TotalProducerDisposalFeeWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(summary.TotalProducerDisposalFee),
                BadDebtProvision = CurrencyConverter.ConvertToCurrency(summary.BadDebtProvision),
                TotalProducerDisposalFeeWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(summary.TotalProducerDisposalFeeWithBadDebtProvision),
                EnglandTotal = CurrencyConverter.ConvertToCurrency(summary.EnglandTotal),
                WalesTotal = CurrencyConverter.ConvertToCurrency(summary.WalesTotal),
                ScotlandTotal = CurrencyConverter.ConvertToCurrency(summary.ScotlandTotal),
                NorthernIrelandTotal = CurrencyConverter.ConvertToCurrency(summary.NorthernIrelandTotal),
                TonnageChangeCount = summary.TonnageChangeCount,
                TonnageChangeAdvice = summary.TonnageChangeAdvice,
            };
        }
    }
}
