using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
namespace EPR.Calculator.Service.Function.Mapper
{
    public class TotalProducerBillWithBadDebtProvisionMapper : ITotalProducerBillWithBadDebtProvisionMapper
    {
        public TotalProducerBillWithBadDebtProvision Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts;
            return new TotalProducerBillWithBadDebtProvision
            {
                TotalProducerBillWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionForTotalProducerBill = CurrencyConverter.ConvertToCurrency(costs.BadDebtProvision),
                TotalProducerBillWithBadDebtProvisionAmount = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.EnglandTotalWithBadDebtProvision),
                WalesTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.WalesTotalWithBadDebtProvision),
                ScotlandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForProducerBillWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.NorthernIrelandTotalWithBadDebtProvision)
            };
        }
    }
}
