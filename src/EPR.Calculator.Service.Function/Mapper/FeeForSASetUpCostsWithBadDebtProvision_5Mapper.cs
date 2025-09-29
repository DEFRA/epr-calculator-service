using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForSaSetUpCostsWithBadDebtProvision5Mapper : IFeeForSaSetUpCostsWithBadDebtProvision5Mapper
    {
        public FeeForSaSetUpCostsWithBadDebtProvision5 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts;
            return new FeeForSaSetUpCostsWithBadDebtProvision5
            {
                TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionFor5 = CurrencyConverter.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0),
            };
        }
    }
}
