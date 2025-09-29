using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForSASetUpCostsWithBadDebtProvision_5Mapper : IFeeForSASetUpCostsWithBadDebtProvision_5Mapper
    {
        public FeeForSASetUpCostsWithBadDebtProvision_5 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts;
            return new FeeForSASetUpCostsWithBadDebtProvision_5
            {
                TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionFor5 = CurrencyConverter.ConvertToCurrency(costs.BadDebtProvision),
                TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.EnglandTotalWithBadDebtProvision),
                WalesTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.WalesTotalWithBadDebtProvision),
                ScotlandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.NorthernIrelandTotalWithBadDebtProvision),
            };
        }
    }
}
