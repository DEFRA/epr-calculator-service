using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Common.Utils;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForCommsCostsWithBadDebtProvision2bMapper : IFeeForCommsCostsWithBadDebtProvision2bMapper
    {
        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB;
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b
            {
                TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionFor2b = CurrencyConverter.ConvertToCurrency(costs.BadDebtProvision),
                TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.EnglandTotalWithBadDebtProvision),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.WalesTotalWithBadDebtProvision),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.NorthernIrelandTotalWithBadDebtProvision)
            };
        }
    }
}