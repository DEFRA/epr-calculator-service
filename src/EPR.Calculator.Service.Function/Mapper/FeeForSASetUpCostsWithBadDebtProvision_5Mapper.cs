using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForSASetUpCostsWithBadDebtProvision_5Mapper : IFeeForSASetUpCostsWithBadDebtProvision_5Mapper
    {
        public FeeForSASetUpCostsWithBadDebtProvision_5 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new FeeForSASetUpCostsWithBadDebtProvision_5
            {
                TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionFor5 = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts.BadDebtProvision),
                TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts.EnglandTotalWithBadDebtProvision),
                WalesTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts.WalesTotalWithBadDebtProvision),
                ScotlandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts.NorthernIrelandTotalWithBadDebtProvision),
            };
        }
    }
}
