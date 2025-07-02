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
                TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeWithoutBadDebtProvisionSection5),
                BadDebtProvisionFor5 = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionSection5),
                TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeWithBadDebtProvisionSection5),
                EnglandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvisionSection5),
                WalesTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvisionSection5),
                ScotlandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvisionSection5),
                NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvisionSection5),
            };
        }
    }
}
