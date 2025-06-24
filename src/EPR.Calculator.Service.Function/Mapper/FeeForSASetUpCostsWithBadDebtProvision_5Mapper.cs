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
                TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerFeeWithoutBadDebtProvisionSection5,
                BadDebtProvisionFor5 = calcResultSummaryProducerDisposalFees.BadDebtProvisionSection5,
                TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerFeeWithBadDebtProvisionSection5,
                EnglandTotalForSASetUpCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvisionSection5,
                WalesTotalForSASetUpCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvisionSection5,
                ScotlandTotalForSASetUpCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvisionSection5,
                NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvisionSection5,
            };
        }
    }
}
