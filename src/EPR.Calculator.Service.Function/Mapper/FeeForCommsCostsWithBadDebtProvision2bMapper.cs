using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Common.Utils;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForCommsCostsWithBadDebtProvision2bMapper : IFeeForCommsCostsWithBadDebtProvision2bMapper
    {
        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b
            {
                TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeWithoutBadDebtFor2bComms),
                BadDebtProvisionFor2b = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2bComms),
                TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeWithBadDebtFor2bComms),
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtFor2bComms),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtFor2bComms),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtFor2bComms),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtFor2bComms)
            };
        }
    }
}