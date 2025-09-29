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
                TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionForLADisposalCosts = CurrencyConverter.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerFeeForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0),
            };
        }
    }
}
