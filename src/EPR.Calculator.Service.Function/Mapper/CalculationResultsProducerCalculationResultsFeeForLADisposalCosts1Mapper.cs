using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper : ICalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper
    {
        public CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.LocalAuthorityDisposalCostsSectionOne;
            return new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
            {
                TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionForLADisposalCosts = CurrencyConverter.ConvertToCurrency(costs.BadDebtProvision),
                TotalProducerFeeForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.EnglandTotalWithBadDebtProvision),
                WalesTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.WalesTotalWithBadDebtProvision),
                ScotlandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.NorthernIrelandTotalWithBadDebtProvision),
            };
        }
    }
}
