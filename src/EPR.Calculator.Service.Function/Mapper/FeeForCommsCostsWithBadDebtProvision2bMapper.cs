using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Common.Utils;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForCommsCostsWithBadDebtProvision2BMapper : IFeeForCommsCostsWithBadDebtProvision2BMapper
    {
        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB;
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B
            {
                TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionFor2b = CurrencyConverter.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0)
            };
        }
    }
}